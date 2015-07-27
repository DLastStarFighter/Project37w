using Project37ServerTester.MediaConsumerService.MessageComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project37ServerTester.MediaConsumerService.TaskComponent
{
    public class RegisterMediaConsumerTask : Task, IMessageHandler
    {
        MessageManager _messageManager;

        public RegisterMediaConsumerTask(MessageManager messageManager)
        {
            _messageManager = messageManager;
        }


        public override void Exectute()
        {
            //Have the message manager send a register to server request

            _messageManager.SendRegisterMediaConsumerMessage();
        }

        public void Kill()
        {
            throw new NotImplementedException();
        }

        public void OnNewMessageEvent(MessageComponent.Messages.Message message)
        {
            //If message is a response, handle it

            //Notify the delegate that task is complete
            throw new NotImplementedException();
        }
    }
}
