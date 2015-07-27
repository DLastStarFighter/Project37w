using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using XenFoundation.XenLog;
using XenNet.Connection;
using XenNet.Connection.Client;
using System.Drawing;
using XenNet.Package;
using System.IO;
using Project37ServerTester.MessageBuilder;
using System.Drawing.Imaging;

namespace Project37ServerTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public delegate void MyDelegate(string str);


    public partial class MainWindow : Window, ILogHandler
    {
        TCPClient _mediaConsumer;
        TCPClient _mediaProducer;

        MyDelegate del;
        Dispatcher userDispatcher;

        public MainWindow()
        {
            InitializeComponent();

            userDispatcher = Dispatcher.CurrentDispatcher;
            del = new MyDelegate(this.DisplayLog);
            _mediaConsumer = new TCPClient();
            _mediaProducer = new TCPClient();
            Log.Register(this);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void DisplayLog(string str)
        {

            LogList.Items.Insert(0, str);
        }
        public void OnLogEvent(LogEvent logEvent)
        {
            //Add log entry to list
            userDispatcher.Invoke(del, new object[] { logEvent.Message });
        }

        private void clientMediaProducerConnect_Click(object sender, RoutedEventArgs e)
        {
            string serverIP = this.serverIPTextBox.Text;
            int port = int.Parse(this.serverPortTextBox.Text);
            _mediaProducer.Connect(serverIP, port);
        }

        private void mediaConsumerConnect_Click(object sender, RoutedEventArgs e)
        {
            string serverIP = this.serverIPTextBox.Text;
            int port = int.Parse(this.serverPortTextBox.Text);
            _mediaConsumer.Connect(serverIP, port);
        }

        private void sendRegisterMessage_Click(object sender, RoutedEventArgs e)
        {
            TCPSendPackage package = new TCPSendPackage();
            package.Data = Encoding.ASCII.GetBytes("<message type=\"register_media_producer\"/>");
            _mediaProducer.Send(package);
        }

        private void sendConsumerRegisterMessage_Click(object sender, RoutedEventArgs e)
        {
            TCPSendPackage package = new TCPSendPackage();
            package.Data = Encoding.ASCII.GetBytes("<message type=\"register_media_consumer\"/>");
            _mediaProducer.Send(package);
        }

        private void sendImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile("../../sample_jpeg.jpg", false);
                Log.info("Loading image was a sucess.");

                ImageMessage message;
                ImageMessageBuilder.BuildImageMessage(image, 0, out message);


                TCPSendPackage package = new TCPSendPackage();

                package.Data = Encoding.ASCII.GetBytes(message.ImageSendRequest);
                _mediaProducer.Send(package);

            }
            catch (FileNotFoundException ex)
            {
                Log.error("Failed to load image: {0}", ex.Message);
            }
            catch (OutOfMemoryException ex)
            {
                Log.error("Failed to load image: {0}", ex.Message);
            }

        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
      
        Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        private void clipboardImage_Click(object sender, RoutedEventArgs e)
        {
          try
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile("../../sample_jpeg.jpg", false);
                Log.info("Loading image was a sucess.");

            using (MemoryStream ms = new MemoryStream(imageToByteArray(image)))
            {
                var decoder = BitmapDecoder.Create(ms,
                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
               // Bitmap bitmap = GetBitmap( decoder.Frames[0]);
                Clipboard.SetImage(decoder.Frames[0]);
            }



             

            }
            catch (FileNotFoundException ex)
            {
                Log.error("Failed to load image: {0}", ex.Message);
            }
            catch (OutOfMemoryException ex)
            {
                Log.error("Failed to load image: {0}", ex.Message);
            }
        }


    }
}
