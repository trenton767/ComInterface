using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsPanel_Interface
{
    public class VATSIM
    {
        public VATgen general;
        public List<VATpilots> pilots = new List<VATpilots>();
        public List<VATcontrollers> controllers = new List<VATcontrollers>();
        public List<VATatis> atis = new List<VATatis>();
        //public List<VATservers> servers = new List<VATservers>();
        //public List<VATprefiles> prefiles = new List<VATprefiles>();
        //public List<VATfacilities> facilities = new List<VATfacilities>();
        //public List<VATratings> ratings = new List<VATratings>();
        //public List<VATpilot_ratings> pilot_ratings = new List<VATpilot_ratings>();
        //public List<VATmilitary_ratings> military_ratings = new List<VATmilitary_ratings>();

        public VATSIM(string inData)
        {
            string[] parts = inData.Split(',');
            dynamic array = JsonConvert.DeserializeObject(inData);
            dynamic gen = array["general"];
            this.general = new VATgen(Convert.ToInt32(gen["version"]), Convert.ToDateTime(gen["update_timestamp"]), Convert.ToInt32(gen["connected_clients"]), Convert.ToInt32(gen["unique_users"]));
            dynamic pil = array["pilots"];
            foreach(dynamic p in pil)
            {
                VATfltpln FltPln;
                //p["flight_plan"]
                dynamic plan = p["flight_plan"];
                if (plan != null)
                {
                    FltPln = new VATfltpln(Convert.ToString(plan["flight_rules"]), Convert.ToString(plan["aircraft"]), Convert.ToString(plan["aircraft_faa"]), Convert.ToString(plan["aircraft_short"]), Convert.ToString(plan["departure"]), Convert.ToString(plan["arrival"]), Convert.ToString(plan["alternate"]), Convert.ToString(plan["deptime"]), Convert.ToString(plan["enroute_time"]), Convert.ToString(plan["fuel_time"]), Convert.ToString(plan["remarks"]), Convert.ToString(plan["route"]), Convert.ToInt32(plan["revision_id"]), Convert.ToString(plan["assigned_transponder"]));
                }
                else
                {
                    FltPln = new VATfltpln("", "", "", "", "", "", "", "", "", "", "", "", 0, "");
                }
                pilots.Add(new VATpilots(Convert.ToInt32(p["cid"]), Convert.ToString(p["name"]), Convert.ToString(p["callsign"]), Convert.ToString(p["server"]), Convert.ToInt32(p["pilot_rating"]), Convert.ToInt32(p["military_rating"]), Convert.ToDouble(p["latitude"]), Convert.ToDouble(p["longitude"]), Convert.ToInt32(p["altitude"]), Convert.ToInt32(p["groundspeed"]), Convert.ToString(p["transponder"]), Convert.ToInt32(p["heading"]), Convert.ToDouble(p["qnh_i_hg"]), Convert.ToDouble(p["qnh_mb"]), FltPln, Convert.ToDateTime(p["logon_time"]), Convert.ToDateTime(p["last_updated"])));
            }
            dynamic cont = array["controllers"];
            foreach(dynamic c in cont)
            {
                string comments = "";
                if (c["text_atis"] != null)
                {
                    foreach (string item in c["text_atis"])
                    {
                        comments = comments + item + " ";
                    }
                }
                controllers.Add(new VATcontrollers(Convert.ToInt32(c["cid"]), Convert.ToString(c["name"]), Convert.ToString(c["callsign"]), Convert.ToString(c["frequency"]), Convert.ToInt32(c["facility"]), Convert.ToInt32(c["rating"]), Convert.ToString(c["server"]), Convert.ToInt32(c["visual_range"]), comments, Convert.ToDateTime(c["last_updated"]), Convert.ToDateTime(c["logon_time"])));
            }
            dynamic ATIS = array["atis"];
            foreach(dynamic a in ATIS)
            {
                string comments = "";
                if (a["text_atis"] != null)
                {
                    foreach (string item in a["text_atis"])
                    {
                        comments = comments + item + " ";
                    }
                }
                Console.WriteLine(a);
                atis.Add(new VATatis(Convert.ToInt32(a["cid"]), Convert.ToString(a["name"]), Convert.ToString(a["callsign"]), Convert.ToString(a["frequency"]), Convert.ToInt32(a["facility"]), Convert.ToInt32(a["rating"]), Convert.ToString(a["server"]), Convert.ToInt32(a["visual_range"]), Convert.ToString(a["atis_code"]), comments, Convert.ToDateTime(a["last_updated"]), Convert.ToDateTime(a["logon_time"])));
            }
        }
    }
}
