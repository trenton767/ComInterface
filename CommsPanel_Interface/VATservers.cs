using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsPanel_Interface
{
    public class VATservers
    {
        string ident;
        string hostname_or_ip;
        string location;
        string name;
        bool client_connections_allowed;
        bool is_sweatbox;

        public VATservers(string ident, string hostname_or_ip, string location, string name, bool client_connections_allowed, bool is_sweatbox)
        {
            this.ident = ident;
            this.hostname_or_ip = hostname_or_ip;
            this.location = location;
            this.name = name;
            this.client_connections_allowed = client_connections_allowed;
            this.is_sweatbox = is_sweatbox;
        }
    }
}
