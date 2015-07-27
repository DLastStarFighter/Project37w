using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenNet.Package;

namespace XenNet.Connection
{
    public interface IDataReceivedEventHandler
    {
        void OnDataRecevied(TCPReceivePackage tcpMessage);
    }

    public interface IConnectionModelDelegate
    {
        void OnConnectionStatusChanged();
    }
}
