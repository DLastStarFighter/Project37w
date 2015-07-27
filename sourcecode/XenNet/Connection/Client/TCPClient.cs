using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XenNet.Package;

namespace XenNet.Connection
{
    public partial class TCPClient: TCPConnection
    {
        private int _port;

        private string _ipAddress;


        public TCPClient()
        {
        }
   

        public void Connect(string ipAddress, int port)
        {
            // Connect to a remote device.
            try
            {
                _ipAddress = ipAddress;
                _port = port;

                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                // IPHostEntry ipHostInfo = Dns.Resolve("host.contoso.com");
                // IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress tcpipAddress = System.Net.IPAddress.Parse(_ipAddress);
                IPEndPoint remoteEP = new IPEndPoint(tcpipAddress, _port);

                // Create a TCP/IP socket.
                _socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                _socket.BeginConnect(remoteEP,
                    new AsyncCallback(this.ConnectCallback), _socket);
                connectDone.WaitOne();

                
                /*
                // Send test data to the remote device.
                Send(client, "This is a test<EOF>");
                sendDone.WaitOne();

                // Receive the response from the remote device.
                Receive(client);
                receiveDone.WaitOne();

                // Write the response to the console.
                Console.WriteLine("Response received : {0}", response);
                */


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        protected override void OnMessageReceived(byte[] messageData)
        {
            //Build the received message package
            TCPReceivePackage package = new TCPReceivePackage();
            package.Data = messageData;

            //Notfiy delegate
            NotifyMessageReceived(package);
        }
    }
}
