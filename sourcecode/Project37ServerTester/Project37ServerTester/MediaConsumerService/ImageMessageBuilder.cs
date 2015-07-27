using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project37ServerTester.MessageBuilder
{
    public class ImageMessage
    {
        private static ulong _messageID = 0;
        public ulong MessageID { get { return _messageID; } set { } }

        public ImageMessage()
        {
            _messageID++;
        }

        public string ImageSendRequest { get; set; }

        public string ImageString { get; set; }
        public byte[] ImageByteArray
        {
            get
            {
                byte[] bytes = new byte[ImageString.Length * sizeof(char)];
                System.Buffer.BlockCopy(ImageString.ToCharArray(), 0, bytes, 0, bytes.Length);
                return bytes;
            }

            set { }
        }
    }

    public class ImageMessageBuilder
    {
        public static void BuildImageMessage(System.Drawing.Image image, long clientID, out ImageMessage message)
        {
            message = new ImageMessage();

            message.ImageString = ImageToBase64(image, System.Drawing.Imaging.ImageFormat.Jpeg);

            message.ImageSendRequest = String.Format("<message client_id=\"{0}\" type=\"image_request\"></message>", clientID);
        }

        public static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
        }

        public static string ImageToBase64(System.Drawing.Image image,
  System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static System.Drawing.Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }
    }

}
