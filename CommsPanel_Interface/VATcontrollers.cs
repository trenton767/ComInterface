using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsPanel_Interface
{
    public class VATcontrollers
    {
        public int cid;
        public string name;
        public string callsign;
        public string frequency;
        public int facility;
        public int rating;
        public string server;
        public int visual_range;
        public string text_atis;
        public DateTime last_updated;
        public DateTime logon_time;

        public VATcontrollers(int cid, string name, string callsign, string frequency, int facility, int rating, string server, int visual_range, string text_atis, DateTime last_updated, DateTime logon_time)
        {
            this.cid = cid;
            this.name = name;
            this.callsign = callsign;
            this.frequency = frequency;
            this.facility = facility;
            this.rating = rating;
            this.server = server;
            this.visual_range = visual_range;
            this.text_atis = text_atis;
            this.last_updated = last_updated;
            this.logon_time = logon_time;
        }
    }
}
