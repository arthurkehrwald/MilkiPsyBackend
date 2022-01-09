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

        protected void SendFeedback(string uniqueFeedbackName, bool goNext)
        {
            FeedbackMessage message = new FeedbackMessage
            {
                currentState = stateListener.State,
                uniqueFeedbackName = uniqueFeedbackName,
                goToNextStage = goNext
            };

            string messageText = JsonConvert.SerializeObject(message);
            server.SendMessageToClient(messageText);
        }
    }

    struct FeedbackMessage
    {
        public ClientState currentState;
        public bool goToNextStage;
        public string uniqueFeedbackName;
    }
}
