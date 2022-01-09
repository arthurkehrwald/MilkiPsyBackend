using System;
using MilkiPsyBackend.Input;

namespace MilkiPsyBackend
{
    class ExampleEvaluator : Evaluator
    {
        private const string FeedbackCommand = "feedback";
        private const string ChangeStageCommand = "changestage";
        private const string ChangeStageNextArg = "next";
        private const string ChangeStagePrevArg = "previous";
        private const string PopupCommand = "popup";        
        private const string CommandParseErrorMessage = "[ExampleEvaluator] Failed to parse arguments of command {0}";

        public ExampleEvaluator(Server server, ClientStateMessageReceiver stateListener) : base(server, stateListener) { }

        public override void Start()
        {
            base.Start();
            CommandLineInterface.Instance.TryAddCommand(FeedbackCommand, FeedbackCommandHandler);
            CommandLineInterface.Instance.TryAddCommand(ChangeStageCommand, ChangeStageCommandHandler);
            CommandLineInterface.Instance.TryAddCommand(PopupCommand, PopupMessageCommandHandler);
            Console.WriteLine("[ExampleEvaluator] Started");
        }

        public override void Stop()
        {
            base.Stop();               
            CommandLineInterface.Instance.TryRemoveCommand(FeedbackCommand);
            CommandLineInterface.Instance.TryRemoveCommand(ChangeStageCommand);
            CommandLineInterface.Instance.TryRemoveCommand(PopupCommand);
            Console.WriteLine("[ExampleEvaluator] Stopped");
        }

        private void FeedbackCommandHandler(string[] args)
        {
            try
            {
                FeedbackMessageData messageData = ParseFeedbackCommand(args);
                SendMessage(messageData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private FeedbackMessageData ParseFeedbackCommand(string[] args)
        {
            if (args.Length != 1)
            {
                string error = string.Format(CommandParseErrorMessage, FeedbackCommand);
                throw new Exception(error);
            }

            FeedbackMessageData result = new()
            {
                jsonFilename = args[0]
            };

            return result;
        }

        private void ChangeStageCommandHandler(string[] args)
        {
            try
            {
                ChangeStageMessageData messageData = ParseChangeStageCommand(args);
                SendMessage(messageData);
                if (messageData.function == ChangeStageMessageData.Function.Next)
                {
                    string[] popupArgs = { "stage_completed.json" };
                    PopupMessageData popupMessageData = ParsePopupCommand(popupArgs);
                    SendMessage(popupMessageData, false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private ChangeStageMessageData ParseChangeStageCommand(string[] args)
        {
            if (args.Length != 1)
            {
                string error = string.Format(CommandParseErrorMessage, ChangeStageCommand);
                throw new Exception(error);
            }

            ChangeStageMessageData result = new();

            switch (args[0])
            {
                case ChangeStagePrevArg:
                    result.function = ChangeStageMessageData.Function.Previous;
                    break;
                case ChangeStageNextArg:
                    result.function = ChangeStageMessageData.Function.Next;
                    break;
                default:
                    if (int.TryParse(args[0], out int index))
                    {
                        if (index >= 0)
                        {
                            result.function = ChangeStageMessageData.Function.SetIndex;
                            result.index = index;
                        }
                    }
                    else
                    {
                        string error = string.Format(CommandParseErrorMessage, ChangeStageCommand);
                        throw new Exception(error);
                    }
                    break;
            }

            return result;
        }

        private void PopupMessageCommandHandler(string[] args)
        {
            try
            {
                PopupMessageData messageData = ParsePopupCommand(args);
                SendMessage(messageData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private PopupMessageData ParsePopupCommand(string[] args)
        {
            if (args.Length != 1)
            {
                string error = string.Format(CommandParseErrorMessage, PopupCommand);
                throw new Exception(error);
            }

            PopupMessageData result = new()
            {
                jsonFileName = args[0]
            };

            return result;
        }
    }
}
