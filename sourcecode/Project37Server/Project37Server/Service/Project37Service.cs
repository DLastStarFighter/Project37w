using Project37Server.Message_Component;
using Project37Server.Participant_Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenFoundation.XenLog;
using XenNet.Connection.Client;
using XenNet.Connection.ClientServerProject;
using XenNet.Package;

namespace Project37Server.Service
{
    class Project37Service : ITCPServerEventHandler, IRemoteClientMessageDelegate
    {
        Participant _mediaProvider;
        Participant _mediaConsumer;
        TCPServer _server;

        Dictionary<uint, RemoteClientMessageService> _remoteClientMessageServices = new  Dictionary<uint, RemoteClientMessageService>();

        public Project37Service(TCPServer server)
        {
            _server = server;
            
        }

        public void Startup()
        {
            Log.info("Attempting to start Project37 Service");

            if(_server == null)
            {
                Log.error("Unable to startup Project37 Service. Server has not been set");
            }
            else
            {
                _server.Register(this);
                _server.Start();
            }
        }

        public void Shutdown()
        {
            if(_server != null)
            {
                _server.Shutdown();
                _server.Unregister(this);
            }
        }

        public void OnRemoteClientAdded(XenNet.Connection.Client.TCPRemoteClient remoteClient)
        {
            //Create a new Message Service and set the remoteClient
            //Add message service to list of message services
            _remoteClientMessageServices.Add(remoteClient.ClientID, new RemoteClientMessageService(remoteClient, this));
        }

        public void OnRemoteClientRemoved(uint remoteClientID)
        {
            //KTODO remove message service from list if exist
            _remoteClientMessageServices.Remove(remoteClientID);

            if(_mediaProvider != null)
            {
                if (_mediaProvider.ConnectionID == remoteClientID)
                {
                    _mediaProvider = null;
                    Log.info("Removed media provider.");
                }
            }
           
            if(_mediaConsumer != null)
            {
                if (_mediaConsumer.ConnectionID == remoteClientID)
                {
                    _mediaConsumer = null;
                    Log.info("Removed media consumer.");
                }
            }
        }

        public void HandleMediaProviderRegistrationMessageReceived(uint connectionID)
        {
            TCPRemoteClient client;
            if(true == _server.GetTCPRemoteClient(connectionID, out client))
            {
                _mediaProvider = new Participant(connectionID);
                Log.info("Media provider set.");
            }
        }

        public void HandleMediaConsumeRegistrationMessageReceived(uint connectionID)
        {
            TCPRemoteClient client;
            if (true == _server.GetTCPRemoteClient(connectionID, out client))
            {
                _mediaConsumer = new Participant(connectionID);
                Log.info("Media consumer set.");
            }
        }

        public void HandleImageMessageReceived(uint connecitonID, byte[] messageData)
        {
            if(_mediaConsumer != null)
            {
                if (connecitonID == _mediaProvider.ConnectionID)
                {
                    TCPRemoteClient client;
                    if (false == _server.GetTCPRemoteClient(_mediaConsumer.ConnectionID, out client))
                    {
                        Log.error("Failed to get media consumer remote client.");
                    }
                    else
                    {
                        TCPSendPackage sendPackage = new TCPSendPackage();
                        sendPackage.Data = messageData;
                        client.Send(sendPackage);
                    }
                }
            }
        }
    }
}
