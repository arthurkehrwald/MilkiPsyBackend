using System;
using System.Net;
using System.Net.Sockets;

namespace MilkiPsyBackend
{
    class Server
    {
        public event EventHandler<string> MessageReceived;
        public event EventHandler ClientDisconnected;

        private const int Port = 13000;
        private const int ReceiveBufferSize = 4096;
        private const int SendBufferSize = 4096;

        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receivedPacket;

        public void Start()
        {
            listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            listener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            
            Console.WriteLine($"[Server] Server started on port {Port}");
        }

        public void SendMessageToClient(string msg)
        {
            if (client == null)
            {
                Console.WriteLine("[Server] Cannot send message, because no client is connected");
                return;
            }

            using Packet packet = new();
            packet.Write(msg);
            packet.WriteLength();
            try
            {
                stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Server] Error sending message to client: {e.Message}");
            }
        }

        private void TcpConnectCallback(IAsyncResult result)
        {
            TcpClient client = listener.EndAcceptTcpClient(result);
            ConnectToClient(client);
        }

        private void ConnectToClient(TcpClient client)
        {
            if (client == null)
            {
                return;
            }

            if (this.client != null)
            {
                Console.WriteLine($"[Server] Failed to connect to Client ({client.Client.RemoteEndPoint})," +
                    $" because another client is already connected");
                return;
            }

            this.client = client;
            this.client.ReceiveBufferSize = ReceiveBufferSize;
            this.client.SendBufferSize = SendBufferSize;

            stream = this.client.GetStream();
            receiveBuffer = new byte[ReceiveBufferSize];
            receivedPacket = new();
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, ReceiveCallback, null);

            Console.WriteLine($"[Server] Connected to client ({client.Client.RemoteEndPoint})");
            SendMessageToClient("Helo wasup");
        }

        private void DisconnectFromClient()
        {
            Console.WriteLine($"[Server] Disconnected from client ({client.Client.RemoteEndPoint})");
            client.Close();
            stream = null;
            receivedPacket = null;
            client = null;
            listener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            MainThreadScheduler.ScheduleAction(() =>
            {
                ClientDisconnected?.Invoke(this, null);
            });
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int receivedByteCount = stream.EndRead(result);
                if (receivedByteCount == 0)
                {
                    DisconnectFromClient();
                    return;
                }

                byte[] receivedBytes = new byte[receivedByteCount];
                Array.Copy(receiveBuffer, receivedBytes, receivedBytes.Length);

                bool wasAllPacketDataParsed = ParsePacketData(receivedBytes);
                receivedPacket.Reset(wasAllPacketDataParsed);

                stream.BeginRead(receiveBuffer, 0, ReceiveBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Server] An Error occured while receiving message from client: {e.Message}");
                DisconnectFromClient();
            }
        }

        private bool ParsePacketData(byte[] data)
        {
            int packetLength = 0;

            receivedPacket.SetBytes(data);

            if (receivedPacket.UnreadLength() >= 4)
            {
                packetLength = receivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= receivedPacket.UnreadLength())
            {
                byte[] packetBytes = receivedPacket.ReadBytes(packetLength);

                MainThreadScheduler.ScheduleAction(() =>
                {
                    using Packet packet = new(packetBytes);
                    string message = packet.ReadString();
                    //Console.WriteLine($"[Server] received message from client: {message}");
                    MessageReceived?.Invoke(this, message);
                });

                packetLength = 0;

                if (receivedPacket.UnreadLength() >= 4)
                {
                    packetLength = receivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            return packetLength <= 1;
        }
    }
}
