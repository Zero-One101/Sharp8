using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Sharp8
{
    class Program
    {
        static void Main(string[] args)
        {
            string gameFile;
            var cpu = new Cpu();

            if (args.Length < 1)
            {
#if !DEBUG
                Console.WriteLine("Please pass a game file as an argument.");
                Console.ReadLine();
                Environment.Exit(1);
#endif
                gameFile = "./ROMs/invaders.c8";
            }
            else
            {
                gameFile = args[0];
            }

            cpu.Init();
            cpu.LoadGame(gameFile);

            while (true)
            {
                cpu.AdvanceCycle();
                Thread.Sleep(10);
            }
        }
    }
}
