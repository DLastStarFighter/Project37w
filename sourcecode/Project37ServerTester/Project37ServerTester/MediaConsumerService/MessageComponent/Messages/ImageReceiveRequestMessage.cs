using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project37ServerTester.MediaConsumerService.MessageComponent.Messages
{
    public class ImageReceiveRequestMessage : Message
    {
        public int ImageSizeInBytes { get; set; }

        public ImageReceiveRequestMessage()
        {
            Type = MessageType.ImageReceiveRequest;
        }
    }
}
