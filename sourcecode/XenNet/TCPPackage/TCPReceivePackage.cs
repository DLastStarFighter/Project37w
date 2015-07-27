using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenNet.Participant;

namespace XenNet.Package
{
    public class TCPReceivePackage : TCPPackage
    {
        private ulong _sendingParticipantID;
        public ulong SendingParticipantID
        {
            get { return _sendingParticipantID; }
            set { _sendingParticipantID = value; }
        }

        private DateTime _timeStamp;
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        private byte[] _messageData;
        public byte[] MessageData
        {
            get { return _messageData; }
            set 
            {
                _messageData = new byte[value.Length];
                Buffer.BlockCopy(value, 0, _messageData, 0, value.Length);
            }
        }
    }
}
