using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_RC.classes
{
    public class SystemInfoClass
    {
        public string OSName { get; set; }
        public string OSVersion { get; set; }
        public string OSBuild { get; set; }
        public string VirtualMemory { get; set; }
        public string Memory { get; set; }
        public Dictionary<int, string> CPUs { get; set; }
    }
}
