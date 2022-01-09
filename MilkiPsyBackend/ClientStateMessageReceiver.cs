using System;
using Newtonsoft.Json;

namespace MilkiPsyBackend
{
    class ClientStateMessageReceiver
    {
        public event EventHandler<ClientState> StatusChanged;
        public ClientState State
        {
            get => state;
            private set
            {
                if (value == state)
                {
                    return;
                }

                state = value;
                Console.WriteLine($"[ClientStatus] Program: {state.uniqueProgramName} Stage: {state.uniqueStageName}");
                StatusChanged?.Invoke(this, state);
            }
        }
        private ClientState state;
        private readonly Server server;

        public ClientStateMessageReceiver(Server server)
        {
            this.server = server;
            server.MessageReceived += MessageReceivedHandler;
            server.ClientDisconnected += ClientDisconnectedHandler;
        }

        private void MessageReceivedHandler(object sender, string message)
        {
            ParseInfo(message);
        }

        private void ClientDisconnectedHandler(object sender, EventArgs args)
        {
            State = new();
        }

        private void ParseInfo(string json)
        {
            State = JsonConvert.DeserializeObject<ClientState>(json);
        }
    }

    public struct ClientState
    {
        public string uniqueProgramName;
        public string uniqueStageName;

        public static bool operator ==(ClientState a, ClientState b)
        {
            return a.uniqueProgramName == b.uniqueProgramName
                && a.uniqueStageName == b.uniqueStageName;
        }

        public static bool operator !=(ClientState a, ClientState b)
        {
            return !(a == b);
        }
    }
}
