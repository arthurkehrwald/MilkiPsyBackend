using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MilkiPsyBackend.Input
{
    class CommandLineInterface
    {
        public static CommandLineInterface Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CommandLineInterface();
                }

                return instance;
            }
        }

        private static CommandLineInterface instance;
        private readonly Dictionary<string, Action<string[]>> commands;

        private CommandLineInterface()
        {
            commands = new();
            Task.Factory.StartNew(Run);
        }

        public bool TryAddCommand(string command, Action<string[]> argumentHandler)
        {
            if (commands.TryAdd(command, argumentHandler))
            {
                return true;
            }

            Console.WriteLine($"[CommandLineInterface] Cannot add command '{command}' because it is already defined");
            return false;
        }

        public bool TryRemoveCommand(string command)
        {
            if (commands.Remove(command))
            {
                return true;
            }

            Console.WriteLine($"[CommandLineInterface] Cannot remove command '{command}' because it is not defined");
            return false;
        }

        private void Run()
        {
            while (true)
            {
                string commandWitArgs = Console.ReadLine();
                string[] split = commandWitArgs.Split(" ");

                if (split.Length == 0)
                {
                    continue;
                }

                string command = split[0];
                commands.TryGetValue(command, out Action<string[]> handler);

                if (handler == null)
                {
                    Console.WriteLine($"[CommandLineInterface] Cannot execute entered command '{command}' because it is not defined");
                    continue;
                }

                string[] args = new string[split.Length - 1];
                Array.Copy(split, 1, args, 0, args.Length);

                MainThreadScheduler.ScheduleAction(() =>
                {
                    handler(args);
                });
            }
        }
    }
}
