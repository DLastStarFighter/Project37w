using Project37ServerTester.MediaConsumerService.MessageComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project37ServerTester.MediaConsumerService.TaskComponent
{
    public class RespondImageReceiveRequestTask : Task
    {
        MessageManager _messageManager;
        int _imageSizeInBytes;

        public RespondImageReceiveRequestTask(MessageManager messageManager, int imageSizeInBytes)
        {
            _messageManager = messageManager;
            _imageSizeInBytes = imageSizeInBytes;
        }

        public void Execute()
        {
            //Setup the message manager to receive an image
            _messageManager.RecieveImage(_imageSizeInBytes);
            
            //Have the message manager send a response
            _messageManager.SendImageReceiveRequestResponse();
        }

        public void Kill()
        {
            throw new NotImplementedException();
        }
    }
}
