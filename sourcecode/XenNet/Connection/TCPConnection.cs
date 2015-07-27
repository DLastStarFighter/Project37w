using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XenFoundation.XenLog;
using XenNet.Package;

namespace XenNet.Connection
{
    public interface IClientMessageEventDelegate
    {
        void OnNewTCPReceivePackage(XenNet.Package.TCPReceivePackage package);
    }

    public interface ITCPConnectionSocetExceptionDelegate
    {
        void OnSocketException(TCPConnection client, SocketException e);
    }


    public partial class TCPConnection
    {

        // ManualResetEvent instances signal completion.
        protected static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        protected static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        protected static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        protected Socket _socket;

        public IClientMessageEventDelegate MessageReceiveDelegate { get; set; }

        protected void NotifyMessageReceived(TCPReceivePackage package)
        {
            MessageReceiveDelegate.OnNewTCPReceivePackage(package);
        }


        protected uint _clientID = 0;
        public uint ClientID { get { return _clientID; } set { } }

        // The response from the remote device.
        private static String response = String.Empty;

        public ITCPConnectionSocetExceptionDelegate SoecketExceptionDelegate{get; set;}

        protected virtual void OnMessageReceived(byte[] messageData) { }

        protected virtual void OnTCPConnectionSocketException(SocketException e)
        {
            SocketError enumuerator = (SocketError)e.ErrorCode;

            Log.error(String.Format("Socket error occured with client ID: {0}  Error: " + enumuerator.ToString(), _clientID));

            if (SoecketExceptionDelegate != null)
            {
                SoecketExceptionDelegate.OnSocketException(this, e);
            }
        }


        public TCPConnection()
        {

        }

        public TCPConnection(Socket socket, ITCPConnectionSocetExceptionDelegate remoteClientReceiveDelegate)
        {
            _socket = socket;
            SoecketExceptionDelegate = remoteClientReceiveDelegate;
        }


        protected void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        protected void Receive(TCPConnection sender)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.sender = sender;
                state.workSocket = _socket;

                // Begin receiving the data from the remote device.
                _socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);

            }
            catch (SocketException e)
            {
                OnTCPConnectionSocketException(e);
            }
            catch (Exception e)
            {
                Log.error(e.ToString());
            }
        }




        protected static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;

            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.

                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.

                    state.messageData = new byte[bytesRead];
                    Buffer.BlockCopy(state.buffer, 0, state.messageData, 0, bytesRead);

                    state.sender.OnMessageReceived(state.messageData);

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.

                    // TCPMessageBuilder.BuildReceivedMessage(out message);
                    state.sender.OnMessageReceived(state.messageData);

                    // Signal that all bytes have been received.
                    receiveDone.Set();


                }

            }
            catch (SocketException e)
            {
                state.sender.OnTCPConnectionSocketException(e);
            }
            catch (Exception e)
            {
                Log.error(e.Message);

            }
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public void Send(TCPSendPackage message)
        {
            if (_socket.Connected)
            {
                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = message.Data;

                StateObject state = new StateObject();
                state.sender = this;
                state.workSocket = _socket;
                // Begin sending the data to the remote device.
                _socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), state);
            }
        }

        protected static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                StateObject state = (StateObject)ar.AsyncState;

                Socket client = state.workSocket;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Log.info("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Log.info(e.ToString());
            }
        }

        public void Disconnect()
        {
            // Release the socket.
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public void StartReceive()
        {
            /*  if(_thread != null)
              {
                  //Thread already exist
                  return;
              }
              else
              {
                  _thread = new Thread(this.doAsyncReceieve);
                  _thread.Start();
              }
             */
            doAsyncReceieve();

        }

        public void StopAsyncReceive()
        {
            /*
            _doAsyncReceive = false;
            if(_thread != null)
            {
                //Doing the abort may not be safe, it may be better to use a receive call
                //with a timeout
               //KTODO _thread.Abort();
                //KTODO _thread.Join();
            }
             * */

        }


        private void doAsyncReceieve()
        {
            Receive(this);
            /*  _doAsyncReceive = true;
              while(_doAsyncReceive)
              {
                  if (!_isReceiving)
                  {
                      Receive(this);
                  }
              }
             */
        }


        private HashSet<IDataReceivedEventHandler> _dataReceivedEventHandlers;

        public void Register(IDataReceivedEventHandler eventHandler)
        {
            if (_dataReceivedEventHandlers == null)
            {
                _dataReceivedEventHandlers = new HashSet<IDataReceivedEventHandler>();
            }

            _dataReceivedEventHandlers.Add(eventHandler);
        }

        public void Unregister(IDataReceivedEventHandler eventHandler)
        {
            if (_dataReceivedEventHandlers != null)
            {
                _dataReceivedEventHandlers.Remove(eventHandler);
            }
        }

        private void NotifyMessageRecieved(TCPReceivePackage message)
        {
            foreach (IDataReceivedEventHandler handler in _dataReceivedEventHandlers)
            {
                handler.OnDataRecevied(message);
            }
        }

    }
}
