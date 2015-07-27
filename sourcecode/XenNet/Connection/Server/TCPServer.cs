using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XenNet.Connection.Client;
using XenFoundation.XenLog;

namespace XenNet.Connection.ClientServerProject
{
    #region Interfaces

    /// <summary>
    /// Interface for handling TCP server events
    /// </summary>
    public interface ITCPServerEventHandler
    {
        void OnRemoteClientAdded(TCPRemoteClient remoteClient);
        void OnRemoteClientRemoved(uint remoteClientID);
    }

    #endregion

    /// <summary>
    /// Provides TCP connection oriented services
    /// </summary>
    public class TCPServer : IClientConnectionListenerDelegate, ITCPConnectionSocetExceptionDelegate
    {
        #region Private member

        private Dictionary<uint, TCPRemoteClient> _clientDirectory = new Dictionary<uint, TCPRemoteClient>();
        private string _ipAddress;
        private int _portNumber;
        private Socket _socket;
        private IPEndPoint _ipEndPoint;
        private Thread _clientConnectionListenerThread;
        private uint _clientIDIndex = 1; //Clients are always start at 1
        private ClientConnectionListener _clientConnectionListener;


        #endregion

        #region ITCPServerEventHandler section

        private HashSet<ITCPServerEventHandler> _ITCPServerEventHandler = new HashSet<ITCPServerEventHandler>();


        public void Register(ITCPServerEventHandler handler) { _ITCPServerEventHandler.Add(handler); }
        public void Unregister(ITCPServerEventHandler handler) { _ITCPServerEventHandler.Remove(handler); }

        private void NotifyRemoteClientAdded(TCPRemoteClient remoteClient)
        {
            foreach (ITCPServerEventHandler handler in _ITCPServerEventHandler)
            {
                handler.OnRemoteClientAdded(remoteClient);
            }
        }

        private void NotifyRemoteClientRemoved(uint remoteClientID)
        {
            foreach (ITCPServerEventHandler handler in _ITCPServerEventHandler)
            {
                handler.OnRemoteClientRemoved(remoteClientID);
            }
        }

        #endregion


        #region Properties

        private bool _isRunning = false;
        public bool IsRunning
        {
            get { return _isRunning; }
            set { }
        }


        #endregion


        #region Public methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="portNumber"></param>
        public TCPServer(string ipAddress,
                     int portNumber)
        {
            _ipAddress = ipAddress;
            _portNumber = portNumber;
        }

        /// <summary>
        /// Starts the server services
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if (_isRunning == true)
            {
                Log.info("Server is already running");
                return true;
            }
            else
            {
                Log.info("Starting TCP Server...");

                _ipEndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _portNumber);

                try
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch (SocketException e)
                {
                    Log.error("Failed to start server.  " + ((SocketError)e.ErrorCode).ToString());
                    return false;
                }

                try
                {
                    _socket.Bind(_ipEndPoint);
                }
                catch (SocketException e)
                {
                    Log.error("Failed to start server.  " + ((SocketError)e.ErrorCode).ToString());
                    return false;
                }

                _clientConnectionListener = new ClientConnectionListener(_socket, this);

                _clientConnectionListenerThread = new Thread(_clientConnectionListener.Listen);

                Log.info("Starting Remote Client Listener service...");
                _clientConnectionListenerThread.Start();
            }

            _isRunning = true;
            return true;
        }

        /// <summary>
        /// Shuts down the server and all of its client connections
        /// </summary>
        public void Shutdown()
        {
            Log.info("Shutting down server");
            if (_clientConnectionListener != null)
            {
                _clientConnectionListener.Stop();

                if (_clientConnectionListenerThread != null)
                {
                    _clientConnectionListenerThread.Join();
                    _clientConnectionListenerThread = null;
                }
            }


            try
            {
                if(_socket != null)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Disconnect(true);
                }
               
            }
            catch (SocketException e)
            {
                //Do nothing
            }

            //TODO stop all async client receives
            foreach (KeyValuePair<uint, TCPRemoteClient> remoteClientPair in _clientDirectory)
            {
                TCPRemoteClient remoteClient = remoteClientPair.Value;

                remoteClient.Disconnect();
                RemoveClient(remoteClient.ClientID);
            }

            _isRunning = false;
        }


        /// <summary>
        /// Retrieves the remote client with the given ID
        /// </summary>
        /// <param name="remoteClientID"></param>
        /// <returns></returns>
        public bool GetTCPRemoteClient(uint remoteClientID, out TCPRemoteClient remoteClient)
        {
            remoteClient = null;

            try
            {
                _clientDirectory.TryGetValue(remoteClientID, out remoteClient);

                return true;
            }
            catch (Exception e)
            {
                Log.debug("Error:" + e.Message);
                return false;
            }
        }

        #endregion


        #region Interface implementation

        /// <summary>
        /// Builds a TCPRemoteClient instance object upon a client connection being established
        /// </summary>
        /// <param name="client">The socket of the remote client that connected</param>
        void IClientConnectionListenerDelegate.OnClientConnected(Socket client)
        {
            //Build the remote client
            TCPRemoteClient tcpClient = new TCPRemoteClient(_clientIDIndex, client, this);

            //Add it to the system
            AddRemoteClient(tcpClient);

            //Increment the ID index
            _clientIDIndex++;

            //Allow the remote client to start receiving
            tcpClient.StartReceive();
        }


        /// <summary>
        /// Upon a socket error on a remote client, it will be shutdown and removed from the system
        /// </summary>
        /// <param name="remoteClient"></param>
        /// <param name="e"></param>
        /// 
        public void OnSocketException(TCPConnection client, SocketException e)
        {
            //KTODO Handle specific exception
            Log.error("Remote Client {0} exception has occured." + ((SocketError)e.ErrorCode).ToString(), client.ClientID);

            client.Disconnect();
            RemoveClient(client.ClientID);
        }

        public void OnServerSocketException(SocketException e)
        {
            Log.error("Server socket exception occured:  " + ((SocketError)e.ErrorCode).ToString());
            Shutdown();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Adds given TCPRemoteClient object to the client directory
        /// </summary>
        /// <param name="client"></param>
        private void AddRemoteClient(TCPRemoteClient remoteClient)
        {
            _clientDirectory.Add(remoteClient.ClientID, remoteClient);
            Log.debug("Client added");
            NotifyRemoteClientAdded(remoteClient);
           
        }

        /// <summary>
        /// Removes a client with the given client ID from the client directory
        /// </summary>
        /// <param name="clientID"></param>
        private void RemoveClient(uint clientID)
        {
            _clientDirectory.Remove(clientID);
            Log.debug("Client removed");
            NotifyRemoteClientRemoved(clientID);
        }

        #endregion




      
    }
}

