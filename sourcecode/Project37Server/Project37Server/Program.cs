using Project37Server.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenFoundation.XenLog;
using XenNet.Connection.ClientServerProject;

namespace Project37Server
{
    class Program
    {
        class LogEventHandler : XenFoundation.XenLog.ILogHandler
        {
            public LogEventHandler()
            {
               Log.Register(this);
            }

            public void OnLogEvent(XenFoundation.XenLog.LogEvent logEvent)
            {
                Console.WriteLine(logEvent.Message);
            }
        }

        static void Main(string[] args)
        {
            LogEventHandler logEventHandler = new LogEventHandler();

            TCPServer server = new TCPServer("197.167.1.24", 8037);

            Project37Service service = new Project37Service(server);

            service.Startup();


            for (; ; ) ;
        }
    }
}
