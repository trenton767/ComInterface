using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsPanel_Interface
{
    public class VATfltpln
    {
        public string flight_rules;
        public string aircraft;
        public string aircraft_faa;
        public string aircraft_short;
        public string departure;
        public string arrival;
        public string alternate;
        public string deptime;
        public string enroute_time;
        public string fuel_time;
        public string remarks;
        public string route;
        public int revision_id;
        public string assigned_transponder;

        public VATfltpln(string flight_rules, string aircraft, string aircraft_faa, string aircraft_short, string departure, string arrival, string alternate, string deptime, string enroute_time, string fuel_time, string remarks, string route, int revision_id, string assigned_transponder)
        {
            this.flight_rules = flight_rules;
            this.aircraft = aircraft;
            this.aircraft_faa = aircraft_faa;
            this.aircraft_short = aircraft_short;
            this.departure = departure;
            this.arrival = arrival;
            this.alternate = alternate;
            this.deptime = deptime;
            this.enroute_time = enroute_time;
            this.fuel_time = fuel_time;
            this.remarks = remarks;
            this.route = route;
            this.revision_id = revision_id;
            this.assigned_transponder = assigned_transponder;
        }
    }
}
