﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommsPanel_Interface
{
    public class VATmilitary_ratings
    {
        int id;
        string short_name;
        string long_name;

        public VATmilitary_ratings(int id, string short_name, string long_name)
        {
            this.id = id;
            this.short_name = short_name;
            this.long_name = long_name;
        }
    }
}
