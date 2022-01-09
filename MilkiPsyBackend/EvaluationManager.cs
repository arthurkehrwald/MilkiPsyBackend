using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkiPsyBackend
{
    class EvaluationManager
    {
        private Server server;
        private ClientStateMessageReceiver clientStateListener;
        private Evaluator currentEvaluator;
        private Evaluator CurrentEvaluator
        {
            get => currentEvaluator;
            set
            {
                if (value == currentEvaluator)
                {
                    return;
                }

                currentEvaluator?.Stop();
                currentEvaluator = value;
                currentEvaluator?.Start();
            }
        }

        public EvaluationManager(Server server, ClientStateMessageReceiver clientStateListener)
        {
            this.server = server;
            this.clientStateListener = clientStateListener;
            clientStateListener.StatusChanged += OnClientStatusChanged;
        }

        public void Tick()
        {
            CurrentEvaluator?.Tick();
        }

        private void OnClientStatusChanged(object sender, ClientState info)
        {
            if (string.IsNullOrWhiteSpace(info.uniqueStageName) || info.uniqueStageName == "none")
            {
                CurrentEvaluator = null;
                return;
            }

            switch (info.uniqueStageName)
            {
                case "some_stage":
                    // Set currentEvaluator to the appropriate class
                    break;
                case "some_other_stage":
                    // Set currentEvaluator to the appropriate class
                    break;
                default:
                    CurrentEvaluator = new ExampleEvaluator(server, clientStateListener);
                    break;
            }
        }
    }
}
