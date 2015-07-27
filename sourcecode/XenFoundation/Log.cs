using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenFoundation.XenLog
{

    public interface ILogHandler
    {
        void OnLogEvent(LogEvent logEvent);
    }

    public class LogEvent
    {
        public enum LOG_EVENT_TYPE
        {
            ERROR,
            INFO,
            DEBUG

        }

        public string Message { get; set; }
        public LOG_EVENT_TYPE Type {get; set;}
    }



    public class Log
    {
        private HashSet<ILogHandler> _logHandlers = new HashSet<ILogHandler>();
       
        private static Log _instance;


        public static void Register(ILogHandler handler)
        {
            GetInstance()._logHandlers.Add(handler);
        }

        public static void Unregister(ILogHandler handler)
        {
            GetInstance()._logHandlers.Remove(handler);
        }

        private void NotifyLogEvent(LogEvent logEvent)
        {
            foreach (ILogHandler handler in _logHandlers)
            {
                handler.OnLogEvent(logEvent);
            }
        }


        public static Log GetInstance()
        {
            if(_instance == null)
            {
                _instance = new Log();
            }

            return _instance;
        }

        public static void error(string message, params object[] args)
        {
            error(String.Format(message, args));
        }

        public static void error(string message)
        {
            LogEvent logEvent = new LogEvent();
            logEvent.Message = message;
            logEvent.Type = LogEvent.LOG_EVENT_TYPE.ERROR;
            GetInstance().NotifyLogEvent(logEvent);
        }

        public static void info(string message, params object[] args)
        {
            info(String.Format(message, args));
        }

        public static void info(string message)
        {
            LogEvent logEvent = new LogEvent();
            logEvent.Message = message;
            logEvent.Type = LogEvent.LOG_EVENT_TYPE.INFO;
            GetInstance().NotifyLogEvent(logEvent);
        }

        public static void debug(string message, params object[] args)
        {
            debug(String.Format(message, args));
        }
        public static void debug(string message)
        {
            LogEvent logEvent = new LogEvent();
            logEvent.Message = message;
            logEvent.Type = LogEvent.LOG_EVENT_TYPE.DEBUG;
            GetInstance().NotifyLogEvent(logEvent);
        }
    }
}
