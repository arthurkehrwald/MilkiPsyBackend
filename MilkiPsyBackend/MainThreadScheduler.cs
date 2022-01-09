using System;
using System.Collections.Generic;

namespace MilkiPsyBackend
{
    public static class MainThreadScheduler
    {
        private static readonly List<Action> executeOnMainThread = new();
        private static readonly List<Action> executeCopiedOnMainThread = new();
        private static bool hasPendingActions = false;

        /// <summary>
        /// Sets an action to be executed on the main thread
        /// </summary>
        public static void ScheduleAction(Action _action)
        {
            if (_action == null)
            {
                Console.WriteLine("[MainThreadScheduler] Cannot schedule execution of null action");
                return;
            }

            lock (executeOnMainThread)
            {
                executeOnMainThread.Add(_action);
                hasPendingActions = true;
            }
        }

        /// <summary>
        /// Executes all code meant to run on the main thread.
        /// NOTE: Call this ONLY from the main thread
        /// </summary>
        public static void ExecutePendingActions()
        {
            if (hasPendingActions)
            {
                executeCopiedOnMainThread.Clear();
                lock (executeOnMainThread)
                {
                    executeCopiedOnMainThread.AddRange(executeOnMainThread);
                    executeOnMainThread.Clear();
                    hasPendingActions = false;
                }

                for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
                {
                    executeCopiedOnMainThread[i]();
                }
            }
        }
    }
}