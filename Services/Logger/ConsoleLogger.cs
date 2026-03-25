using System;
using System.Composition;
using System.Runtime.InteropServices;

namespace Services.Logger
{
    [Export(typeof(ILogger))]
    public class ConsoleLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"INFO: {message}");
            Console.ResetColor();
        }

        public void LogErr(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {message}");
            Console.ResetColor();
        }

        public void LogWar(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARNING: {message}");
            Console.ResetColor();
        }
    }

}
