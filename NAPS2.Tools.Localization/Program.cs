using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Localization
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
            }

            string command = args[0];
            if (command == "templates")
            {
                if (args.Length != 1)
                {
                    PrintUsage();
                }

                Templates.Update();
            }
            else if (command == "language")
            {
                if (args.Length != 2)
                {
                    PrintUsage();
                }

                Language.Update(args[1]);
            }
            else
            {
                PrintUsage();
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("NAPS2.Localization.exe templates");
            Console.WriteLine("NAPS2.Localization.exe language fr");
            Environment.Exit(0);
        }
    }
}
