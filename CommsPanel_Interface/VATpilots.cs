using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsPanel_Interface
{
    public class VATpilots
    {
        public int cid;
        public string name;
        public string callsign;
        public string server;
        public int pilot_rating;
        public int military_rating;
        public double latitude;
        public double longitude;
        public int altitude;
        public int groundspeed;
        public string transponder;
        public int heading;
        public double qnh_i_hg;
        public double qnh_mb;
        public VATfltpln flight_plan;
        public DateTime logon_time;
        public DateTime last_updated;

        public VATpilots(int cid, string name, string callsign, string server, int pilot_rating, int military_rating, double latitude, double longitude, int altitude, int groundspeed, string transponder, int heading, double qnh_i_hg, double qnh_mb, VATfltpln flight_plan, DateTime logon_time, DateTime last_updated)
        {
            this.cid = cid;
            this.name = name;
            this.callsign = callsign;
            this.server = server;
            this.pilot_rating = pilot_rating;
            this.military_rating = military_rating;
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
            this.groundspeed = groundspeed;
            this.transponder = transponder;
            this.heading = heading;
            this.qnh_i_hg = qnh_i_hg;
            this.qnh_mb = qnh_mb;
            this.flight_plan = flight_plan;
            this.logon_time = logon_time;
            this.last_updated = last_updated;
        }
    }
}
