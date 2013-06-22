using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Log
{
    public class LogManager
    {
        private LinkedList<LogMessage> log;

        public LogManager()
        {
            log = new LinkedList<LogMessage>();
        }

        public void Log(string message)
        {
            Log(new LogMessage(message));
        }

        public void Log(DateTime time, string message)
        {
            Log(new LogMessage(time, message));
        }

        public void Log(LogLevel level, string message)
        {
            Log(new LogMessage(level, message));
        }

        public void Log(DateTime time, LogLevel level, string message)
        {
            Log(new LogMessage(time, level, message));
        }

        public void Log(LogMessage logMessage)
        {
            lock (log)
            {
                log.AddFirst(logMessage);
            }
        }

        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void LogDebug(DateTime time, string message)
        {
            Log(time, LogLevel.Debug, message);
        }

        public void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void LogInfo(DateTime time, string message)
        {
            Log(time, LogLevel.Info, message);
        }

        public void LogWarn(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public void LogWarn(DateTime time, string message)
        {
            Log(time, LogLevel.Warn, message);
        }

        public void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void LogError(DateTime time, string message)
        {
            Log(time, LogLevel.Error, message);
        }

        public void LogFatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        public void LogFatal(DateTime time, string message)
        {
            Log(time, LogLevel.Fatal, message);
        }

        public IEnumerator<LogMessage> GetEnumerator()
        {
            return log.GetEnumerator();
        }
    }
}
