using System;
using MilkiPsyBackend.Input;

namespace MilkiPsyBackend
{
    class ExampleEvaluator : Evaluator
    {
        private const string FeedbackCommand = "sendfeedback";
        private const string GoNextArg = "gonext";
        private const string CommandParseErrorMessage = "[ExampleEvaluator] Failed to parse arguments of command '" + FeedbackCommand + "'";

        public ExampleEvaluator(Server server, ClientStateMessageReceiver stateListener) : base(server, stateListener) { }

        public override void Start()
        {
            base.Start();
            CommandLineInterface.Instance.TryAddCommand(FeedbackCommand, ReceivedSendCommandHandler);
            Console.WriteLine("[ExampleEvaluator] Started");
        }

        public override void Stop()
        {
            base.Stop();               
            CommandLineInterface.Instance.TryRemoveCommand(FeedbackCommand);
            Console.WriteLine("[ExampleEvaluator] Stopped");
        }

        private void ReceivedSendCommandHandler(string[] args)
        {
            try
            {
                FeedbackCommandParseResult result = ParseFeedbackCommand(args);
                SendFeedback(result.feedbackUniqueName, result.goToNextStage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private FeedbackCommandParseResult ParseFeedbackCommand(string[] args)
        {
            if (args.Length != 1 && args.Length != 2)
            {
                throw new Exception(CommandParseErrorMessage);
            }

            FeedbackCommandParseResult result = new();

            if (args[0] == GoNextArg)
            {
                result.goToNextStage = true;
                if (args.Length == 2)
                {
                    result.feedbackUniqueName = args[1];
                }
            }
            else
            {
                result.feedbackUniqueName = args[0];
                if (args.Length == 2 && args[1] == GoNextArg)
                {
                    result.goToNextStage = true;
                }
            }

            return result;
        }

        private struct FeedbackCommandParseResult
        {
            public string feedbackUniqueName;
            public bool goToNextStage;
        }
    }
}
