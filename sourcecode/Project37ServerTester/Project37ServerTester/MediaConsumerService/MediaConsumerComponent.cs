using Project37ServerTester.MediaConsumerService.MessageComponent;
using Project37ServerTester.MediaConsumerService.MessageComponent.Messages;
using Project37ServerTester.MediaConsumerService.TaskComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenFoundation.XenLog;
using XenNet.Connection;

namespace Project37ServerTester.MediaConsumerService
{
    class MediaConsumerComponent : IMessageHandler
    {

        MessageManager _messageManager;
        TCPClient _client;

        public void Startup()
        {
            //Initialize
            _client = new TCPClient();
            _messageManager = new MessageManager();

            _client.MessageReceiveDelegate = _messageManager;
            _messageManager.setTCPClient(_client);

            //Connect and register with server
            _client.Connect("10.211.55.3", 6666);

            RegisterMediaConsumerTask registertask = new RegisterMediaConsumerTask(_messageManager);
            registertask.Exectute();
            

        }

        public void Shutdown()
        {

        }

        void IMessageHandler.OnNewMessageEvent(MessageComponent.Messages.Message message)
        {
           switch(message.Type)
           {
               case MessageType.ImageReceiveRequest:
                   ImageReceiveRequestMessage requestMessage = message as ImageReceiveRequestMessage;
                   if(requestMessage != null)
                   {
                       RespondImageReceiveRequestTask task = new RespondImageReceiveRequestTask(_messageManager, requestMessage.ImageSizeInBytes);
                       task.Exectute();
                   }
                   else
                   {
                       Log.error("MediaConsumerComponent - failed to cast ImageMessage");
                   }

                   break;
               default:
                   break;
           }
        }
    }
}
