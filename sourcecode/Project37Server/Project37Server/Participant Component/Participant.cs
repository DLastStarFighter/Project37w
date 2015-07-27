using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project37Server.Participant_Component
{
    class Participant
    {
        public uint ConnectionID { get; set; }

        public Participant(uint connectionID)
        {
            this.ConnectionID = connectionID;
        }
    }
}
