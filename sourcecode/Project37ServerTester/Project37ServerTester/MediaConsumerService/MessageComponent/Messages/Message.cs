using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project37ServerTester.MediaConsumerService.MessageComponent.Messages
{
    public enum MessageType
    {
        Unknown,
        RegisterMessageResponse,
        ImageReceiveRequest
    }

    public class Message
    {
        private MessageType _messageType = MessageType.Unknown;
        public MessageType Type 
        {
            get { return _messageType; }
            set{_messageType = value;}
        }
    }

}
