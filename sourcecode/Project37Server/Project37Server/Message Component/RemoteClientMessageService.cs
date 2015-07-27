using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XenFoundation.XenLog;
using XenNet.Connection;
using XenNet.Connection.Client;
using XenNet.Connection.ClientServerProject;

namespace Project37Server.Message_Component
{

    public interface IRemoteClientMessageDelegate
    {
        void HandleMediaProviderRegistrationMessageReceived(uint connecitonID);
        void HandleMediaConsumeRegistrationMessageReceived(uint connecitonID);
        void HandleImageMessageReceived(uint clientID, byte[] messageData);
    }

    public class RemoteClientMessageService : IClientMessageEventHandler
    {
        TCPRemoteClient _remoteClient;

        IRemoteClientMessageDelegate _remoteClientMessageDelegate;

        public RemoteClientMessageService(TCPRemoteClient remoteClient, IRemoteClientMessageDelegate remoteClientMessageDelegate)
        {
            _remoteClient = remoteClient;
            //KTODO may be better to have a start/stop method on this so that it can unregister
            //from the given remote client
            remoteClient.Register(this);
            _remoteClientMessageDelegate = remoteClientMessageDelegate;
        }

        public void OnNewTCPReceivePackage(TCPReceivePackage package)
        {
            Log.info("New message received");

            //KTODO process TCP package
            var strData = Encoding.Unicode.GetString(package.Data);
            using (XmlReader reader = XmlReader.Create(new StringReader(strData)))
            {
                if (reader.ReadToFollowing("message"))
                {
                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.Name)
                        {
                            case "type":
                                {
                                    if (reader.Value == "register_media_provider_client")
                                    {
                                        Log.info("Message is register_media_provider_client");
                                        _remoteClientMessageDelegate.HandleMediaProviderRegistrationMessageReceived(package.ConnectionID);
                                    }
                                    else if (reader.Value == "register_media_consumer_client")
                                    {
                                        Log.info("Message is register_consumer_client");
                                        _remoteClientMessageDelegate.HandleMediaConsumeRegistrationMessageReceived(package.ConnectionID);
                                    }
                                    else if (reader.Value == "image")
                                    {
                                        _remoteClientMessageDelegate.HandleImageMessageReceived(package.ConnectionID, package.Data);
                                    }
                                    else
                                    {
                                        Log.info(String.Format("Message type {0} not handled", reader.Value));
                                    }
                                }
                                break;
                            default:
                                Log.info("Message type is unknown. Will not be processed.");
                                break;
                        }
                    }
                }

            }
        }

        public void Send()
        {

        }
    }
}
