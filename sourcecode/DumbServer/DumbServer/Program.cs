using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenNet.Connection.Client;
using XenNet.Connection.ClientServerProject;
using XenFoundation.XenLog;
using XenNet.Package;
using XenNet.Connection;

namespace ServerTestTool
{

    class LogHandler : ILogHandler
    {
        public void StartUp()
        {
            Log.Register(this);
        }

        public void ShutDown()
        {
            Log.Unregister(this);
        }

        public void OnLogEvent(LogEvent logEvent)
        {
            Console.WriteLine(logEvent.Message);
        }
    }

    class ReceiveHandler : IClientMessageEventHandler, ITCPServerEventHandler
    {
        public void OnNewTCPReceivePackage(XenNet.Connection.TCPReceivePackage package)
        { 
            if(package.Data.Length < 200)
            {
                Console.WriteLine(System.Text.ASCIIEncoding.Default.GetString(package.Data));
            }
            else
            {
                Console.WriteLine("Received package of {0} bytes", package.Data.Length);
            }
           
        }

        public void OnRemoteClientAdded(TCPRemoteClient remoteClient)
        {
            remoteClient.Register(this);
        }

        public void OnRemoteClientRemoved(uint remoteClientID)
        {
          //Do nothing
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            LogHandler myLogHandler = new LogHandler();

            myLogHandler.StartUp();

            ReceiveHandler handler = new ReceiveHandler();
            Console.WriteLine("Server has started.");
            TCPServer server = new TCPServer("10.211.55.3", 6666);
      
            server.Register(handler);
            if(false == server.Start())
            {
                Log.info("Server failed to start.");
                for (; ; ) ;
            }

          
            for (; ; )
            {
                TCPSendPackage message = new TCPSendPackage();
                Console.ReadLine();
                Encoding ascii = Encoding.ASCII;
                Encoding unicode = Encoding.Unicode;
                byte[] unicodeBytes = unicode.GetBytes("This is  a test from server <EOF>");
                byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);
                message.Data = asciiBytes;
                TCPRemoteClient remoteClient;
                if(server.GetTCPRemoteClient(1, out remoteClient))
                {
                    remoteClient.Send(message);
                }
    
                //Thread.Sleep(1000);
                //Console.ReadLine();
                //message = new TCPSendPackage();
                //message.Data = new byte[0];
                //client.Send(message);
            }
            myLogHandler.ShutDown();
        }


    }
}
