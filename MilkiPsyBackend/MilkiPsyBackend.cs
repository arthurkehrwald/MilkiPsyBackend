using System;
using System.Threading; 

namespace MilkiPsyBackend
{
    class MilkiPsyBackend
    {
        private const int TicksPerSec = 30;

        private static bool isRunning = false;
        private static EvaluationManager evaluationManager;

        static void Main(string[] args)
        {
            Console.Title = "Milki-Psy Backend";
            isRunning = true;
            Thread mainThread = new(new ThreadStart(MainThread));
            mainThread.Start();
            Server server = new();
            server.Start();
            ClientStateMessageReceiver clientStatus = new(server);
            evaluationManager = new(server, clientStatus);
            Console.ReadKey();
        }

        private static void MainThread()
        {
            Console.WriteLine($"[Program] Main thread started running at {TicksPerSec} ticks per second");
            DateTime nextUpdateTime = DateTime.Now;

            while (isRunning)
            {
                MainThreadScheduler.ExecutePendingActions();
                nextUpdateTime = nextUpdateTime.AddMilliseconds(1000 / TicksPerSec);
                TimeSpan timeUntilNextUpdate = nextUpdateTime - DateTime.Now;
                if (timeUntilNextUpdate > TimeSpan.Zero)
                {
                    Thread.Sleep(timeUntilNextUpdate);
                }
            }
        }
    }
}
