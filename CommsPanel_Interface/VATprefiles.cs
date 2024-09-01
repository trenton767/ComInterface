using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsPanel_Interface
{
    public class VATprefiles
    {
        int cid;
        string name;
        string callsign;
        VATfltpln flight_plan;
        string last_updated;

        public VATprefiles(int cid, string name, string callsign, VATfltpln flight_plan, string last_updated)
        {
            this.cid = cid;
            this.name = name;
            this.callsign = callsign;
            this.flight_plan = flight_plan;
            this.last_updated = last_updated;
        }
    }
}
