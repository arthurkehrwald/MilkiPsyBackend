using Newtonsoft.Json;

namespace MilkiPsyBackend
{
    abstract class Evaluator
    {
        private readonly Server server;
        private readonly ClientStateMessageReceiver stateListener;        

        public Evaluator(Server server, ClientStateMessageReceiver stateListener)
        {
            this.server = server;
            this.stateListener = stateListener;
        }

        public virtual void Start() { }
        public virtual void Tick() { }
        public virtual void Stop() { }

        protected void SendMessage(IMessageData messageData, bool ignoreMessageIfStateOutdated = true)
        {
            using ToClientPacket packet = new(messageData, stateListener.State, ignoreMessageIfStateOutdated);
            server.SendPacketToClient(packet);
        }
    }
}
