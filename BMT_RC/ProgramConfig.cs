using HawkSync_RC.classes;
using Salaros.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HawkSync_RC
{
    public static class ProgramConfig
    {
        public static string RemoteINI = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Babstats", "Remote Control", "remote.ini");
        public static string RemoteProfiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Babstats", "Remote Control", "profiles.ini");
        public static int timeOut = 50000;
        public static Encoding Encoder = Encoding.Default;
        public static string version = "1.0.0";
        public static List<RCProfile> RCProfiles = new List<RCProfile>();
    }
}
