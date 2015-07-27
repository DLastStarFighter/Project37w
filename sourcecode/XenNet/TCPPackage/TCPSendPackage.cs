using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenNet.Package
{
    public class TCPSendPackage : TCPPackage
    {
        ulong _participantID = 0;
        public ulong ParticipantID
        {
            get { return _participantID; }
            set { _participantID = value; }
        }
    }
}
