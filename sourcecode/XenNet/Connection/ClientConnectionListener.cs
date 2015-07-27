using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using XenFoundation.XenLog;

namespace XenNet.Connection
{

    public interface IClientConnectionListenerDelegate
    {
        void OnClientConnected(Socket client);
        void OnServerSocketException(SocketException e);
    }

    class ClientConnectionListener
    {
        const int MAX_ALLOWED_QUED_CONNECTIONS = 10;

        private Socket _serverSocket;
        private IClientConnectionListenerDelegate _connectionDelegate;

        private bool _run = false;
        private Object _run_lock = new Object();

        private Object _onClientConnected_lock = new Object();
        

        public ClientConnectionListener(Socket serverSocket, IClientConnectionListenerDelegate connectionDelegate)
        {
            _serverSocket = serverSocket;
            _connectionDelegate = connectionDelegate;
        }

        public void Listen()
        {

            Log.info("Remote Client Listener service started.");

            if(_serverSocket != null)
            {
                lock(_run_lock)
                {
                    _run = true;
                }
               

                while (_run)
                {

                    try
                    {
                        Log.info("Listening for new remote client connection...");

                        _serverSocket.Listen(MAX_ALLOWED_QUED_CONNECTIONS);

                        Socket client = _serverSocket.Accept();

                        lock (_onClientConnected_lock)
                        {
                            _connectionDelegate.OnClientConnected(client);
                        }
                    }
                    catch (SocketException e)
                    {
                        if(_connectionDelegate != null)
                        {
                            _connectionDelegate.OnServerSocketException(e);  
                        }
                       
                      
                    }
                }
               
            }
        }

        public void Stop()
        {
            lock(_run_lock)
            {
                _run = false;
            }
           
        }

    }
}
