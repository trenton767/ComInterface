using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsPanel_Interface
{
    public class VATgen
    {
        int version;
        DateTime update_timestamp;
        int connected_clients;
        int unique_users;

        public VATgen(int version, DateTime update_timestamp, int connected_clients, int unique_users)
        {
            this.version = version;
            this.update_timestamp = update_timestamp;
            this.connected_clients = connected_clients;
            this.unique_users = unique_users;
        }
    }
}
