using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenNet.Package
{
    public class TCPPackage
    {


        private byte[] _data;
        public byte[] Data
        {
            get { return _data; }

            set 
            {
                _data = new byte[value.Length];

                Buffer.BlockCopy(value, 0, _data, 0, value.Length);

            }
        }


        public TCPPackage()
        {   
        }

        public TCPPackage(byte[] data)
        {
            Data = data;
        }
        

    }
}
