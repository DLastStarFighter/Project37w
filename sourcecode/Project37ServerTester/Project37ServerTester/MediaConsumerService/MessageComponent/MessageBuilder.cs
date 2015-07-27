using Project37ServerTester.MediaConsumerService.MessageComponent.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XenFoundation.XenLog;
using XenNet.Package;

namespace Project37ServerTester.MediaConsumerService.MessageComponent
{
    public class MessageBuilder
    {
        public static void BuildRegisterMediaConsumerTCPMessage(out TCPSendPackage tcpPackage)
        {
            tcpPackage = new TCPSendPackage();

            tcpPackage.Data = Encoding.ASCII.GetBytes(String.Format("<message type=\"register_media_consumer\"/>"));
        }

        public static void BuildImageReceiveRequestAckTCPMessage(out TCPSendPackage tcpPackage)
        {
            tcpPackage = new TCPSendPackage();

            tcpPackage.Data = Encoding.ASCII.GetBytes(String.Format("<message type=\"image_receive_request_response_ack\"/>"));
        }

        private static Message BuildRegisterMessageResponse()
        {
            Message message = new Message();
            message.Type = MessageType.RegisterMessageResponse;
            return message;
        }

        private static Message BuildImageReceiveRequestMessage(XmlReader reader)
        {
            ImageReceiveRequestMessage message = new ImageReceiveRequestMessage();

            if (!reader.ReadToFollowing("image"))
            {
                Log.debug("MessageBuilder - Unable to parse image message");
                return null;
            }
            else
            {
                  while (reader.MoveToNextAttribute())
                    {
                        switch (reader.Name)
                        {
                            case "image_size":
                                {
                                   try
                                   {
                                       message.ImageSizeInBytes = int.Parse(reader.Value);
                                   }
                                    catch
                                   {
                                        Log.error("MessageBuilder - failed to parse image_size");
                                   }
                                }
                                break;
                            default:
                                break;
                        }
                    }
            }

            return message;
        }



        public static bool BuildImageMessage(byte[] imageData, out ImageMessage message)
        {
            message = new ImageMessage();
            message.ImageData = imageData;
            return true;
        }

        public static bool BuildReceiveMessage(TCPReceivePackage package, out Message message)
        {
            message = null;
            bool bReturn = true;

            //Get message type
            var strData = Encoding.ASCII.GetString(package.Data);
            using (XmlReader reader = XmlReader.Create(new StringReader(strData)))
            {
                if (!reader.ReadToFollowing("message"))
                {
                    Log.debug("Unknown message received.");
                    return false;
                }
                else
                {
                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.Name)
                        {
                            case "type":
                                {
                                    if (reader.Value == "register_media_consumer_response")
                                    {
                                        Log.info(String.Format("Received message: {0}", reader.Value));
                                        message = BuildRegisterMessageResponse();
                                        
                                    }
                                    else if(reader.Value == "image_receive_request")
                                    {
                                        Log.info(String.Format("Received message: {0}", reader.Value));
                                        message = BuildRegisterMessageResponse();
                                    }
                                    else
                                    {
                                        Log.info(String.Format("Message type {0} not handled", reader.Value));
                                        return false;
                                    }
                                }
                                break;
                            default:
                                Log.info("Message type is unknown. Will not be processed.");
                                return false;
                                break;
                        }
                    }
                }

            }

            return bReturn;
        }
    }

}
