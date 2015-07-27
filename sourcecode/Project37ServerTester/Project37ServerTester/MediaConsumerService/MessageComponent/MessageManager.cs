using Project37ServerTester.MediaConsumerService.MessageComponent.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenNet.Connection;
using XenNet.Package;

namespace Project37ServerTester.MediaConsumerService.MessageComponent
{
    public interface IMessageHandler
    {
        void OnNewMessageEvent(Message message);
    }

    public class MessageManager : IClientMessageEventDelegate, IMessageReceiveDelegate
    {
        private TCPClient _client;
        private MessageReceiveProcessor _messageReceiveProcessor = new MessageReceiveProcessor();

        private HashSet<IMessageHandler> _messageEventHandlers = new HashSet<IMessageHandler>();
        public void RegisterMessageEventHandler(IMessageHandler handler)
        {
            _messageEventHandlers.Add(handler);
        }

        public void UnregisterMessageEventHandler(IMessageHandler handler)
        {
            _messageEventHandlers.Remove(handler);
        }

        private void NotifyNewMessageReceived(Message message)
        {
            foreach(IMessageHandler handler in _messageEventHandlers)
            {
                handler.OnNewMessageEvent(message);
            }
        }
        
        public MessageManager()
        {
            _messageReceiveProcessor = new MessageReceiveProcessor();
            _messageReceiveProcessor.MessageReceiveDelegate = this;
        }

        public void setTCPClient(TCPClient client)
        {
            _client = client;
        }

        public void SendRegisterMediaConsumerMessage()
        {
            //Build RegisterMediaConsumerMessage
            TCPSendPackage package;
            MessageBuilder.BuildRegisterMediaConsumerTCPMessage(out package);
            
            //Send it through the client
            _client.Send(package);
        }

        public void SendImageReceiveRequestResponse()
        {
            //Build RegisterMediaConsumerMessage
            TCPSendPackage package;
            MessageBuilder.BuildImageReceiveRequestAckTCPMessage(out package);

            //Send it through the client
            _client.Send(package);
        }

        public void RecieveImage(int nBytes)
        {
            //Tell the message processor to start receiving an image
            _messageReceiveProcessor.ReceiveImage(nBytes);
        }

        public void StopImageReceive()
        {
            //Tell the message processor to stop processing new messages as image data
        }

        void IMessageReceiveDelegate.HandleMessageReceived(Message message)
        {
            NotifyNewMessageReceived(message);
        }

        void IClientMessageEventDelegate.OnNewTCPReceivePackage(TCPReceivePackage package)
        {
            _messageReceiveProcessor.HandleNewMessageData(package);
        }
    }
}
