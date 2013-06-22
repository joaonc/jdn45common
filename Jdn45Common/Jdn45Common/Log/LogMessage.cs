using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Log
{
    public class LogMessage
    {
        private static readonly LogLevel defaultLevel = LogLevel.Info;

        private DateTime time;
        private LogLevel level;
        private string message;

        public LogMessage(string message)
        {
            Time = DateTime.Now;
            Level = defaultLevel;
            Message = message;
        }

        public LogMessage(DateTime time, string message)
        {
            Time = time;
            Level = defaultLevel;
            Message = message;
        }

        public LogMessage(LogLevel level, string message)
        {
            Time = DateTime.Now;
            Level = level;
            Message = message;
        }

        public LogMessage(DateTime time, LogLevel level, string message)
        {
            Time = time;
            Level = level;
            Message = message;
        }

        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }

        public LogLevel Level
        {
            get { return level; }
            set { level = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
