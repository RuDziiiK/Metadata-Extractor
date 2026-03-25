namespace Services.Logger
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWar(string message);
        void LogErr(string message);
    }
}

