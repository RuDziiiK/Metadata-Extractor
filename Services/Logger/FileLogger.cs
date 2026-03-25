using System;
using System.IO;
using System.Composition;

namespace Services.Logger
{
    [Export(typeof(ILogger))]
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();

        public FileLogger()
        {
            _filePath = "C:\\Users\\Fillo\\source\\repos\\MetaData\\Logs\\log.txt";
        }

        public void LogInfo(string message) => Log("INFO", message);
        public void LogWar(string message) => Log("WARN", message);
        public void LogErr(string message) => Log("ERROR", message);

        private void Log(string level, string message)
        {
            lock (_lock)
            {
                File.AppendAllText(_filePath, $"{DateTime.Now} [{level}] {message}{Environment.NewLine}");
            }
        }
    }
}
