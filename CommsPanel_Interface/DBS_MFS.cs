using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsPanel_Interface
{
    public class DBS_MFS
    {
        public string deviceName;
        public SerialPort COM;
        int type;

        public DBS_MFS(string deviceName, SerialPort COM)
        {
            this.deviceName = deviceName;
            this.COM = COM;
        }
    }
}
