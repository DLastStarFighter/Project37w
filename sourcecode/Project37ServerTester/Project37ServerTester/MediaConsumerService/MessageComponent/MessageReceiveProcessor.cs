using Project37ServerTester.MediaConsumerService.MessageComponent.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenNet.Package;

namespace Project37ServerTester.MediaConsumerService.MessageComponent
{
    public interface IMessageReceiveDelegate
    {
        void HandleMessageReceived(Message message);
    }

  

    public class MessageReceiveProcessor
    {

        byte[] imageBuffer;

        public IMessageReceiveDelegate MessageReceiveDelegate { get; set; }
        private void NotifyMessageReceived(Message message)
        {
            if(MessageReceiveDelegate != null)
            {
                MessageReceiveDelegate.HandleMessageReceived(message);
            }
        }
        int _nByteReceiveCount = 0;

        public void ReceiveImage(int nBytes)
        {
            _nByteReceiveCount = nBytes;
        }

        public void StopImageReceive()
        {
            _nByteReceiveCount = 0;
        }

        private byte[] AppendBytes(byte[] baseArray, byte[] appendArray)
        {
            byte[] retArray;
            if (baseArray == null)
            {
                retArray = new byte[appendArray.Length];
                Buffer.BlockCopy(appendArray, 0, retArray, 0, appendArray.Length);
            }
            else
            {
                retArray = new byte[baseArray.Length + appendArray.Length];
                Buffer.BlockCopy(baseArray, 0, retArray, 0, baseArray.Length);
                Buffer.BlockCopy(appendArray, 0, retArray, baseArray.Length, appendArray.Length);
            }

            return retArray;
        }

        public void HandleNewMessageData(TCPReceivePackage package)
        {
            if(_nByteReceiveCount == 0)
            {
                //Attempt to build message from package
                Message message;
                MessageBuilder.BuildReceiveMessage(package, out message);
                NotifyMessageReceived(message);
            }
            else
            {
                imageBuffer = AppendBytes(imageBuffer, package.Data);

                if(imageBuffer.Length == _nByteReceiveCount)
                {
                    //Done receiving image
                    //Build image message
                    ImageMessage message;
                    MessageBuilder.BuildImageMessage(imageBuffer, out message);
                    NotifyMessageReceived(message);

                    //Set the receiving count to to switch the message processor mode back to regular message processing
                    _nByteReceiveCount = 0;
                }
                else
                {
                    //Contiue processing the receipt of an image
                }
            }
        }
    }
}
