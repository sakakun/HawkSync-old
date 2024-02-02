using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_RC.classes
{
    public class RCProfile
    {
        public string ProfileName { get; set; }
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
