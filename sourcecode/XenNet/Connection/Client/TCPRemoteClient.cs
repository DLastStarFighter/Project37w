using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XenFoundation.XenLog;
using XenNet.Package;
using XenNet.Participant;

namespace XenNet.Connection.Client
{
    
    public class TCPRemoteClient : TCPConnection
    {
    

   
        

        public TCPRemoteClient( uint clientID, 
                                Socket clientSocket, 
                                ITCPConnectionSocetExceptionDelegate clientExceptionHandler) : base(clientSocket, clientExceptionHandler)
        {
            _clientID = clientID;
        }

         

        protected override void OnMessageReceived(byte[] messageData)
        {
            //Build the received message package
            TCPReceivePackage package = new TCPReceivePackage();
            package.Data = messageData;
            package.SendingParticipantID = _clientID;

            //Notfiy delegate
            NotifyMessageReceived(package);

        }

      


    }
}
