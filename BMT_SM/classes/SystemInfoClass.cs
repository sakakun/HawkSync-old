using System.Collections.Generic;

namespace HawkSync_SM.classes
{
    public class SystemInfoClass
    {
        public string OSName { get; set; }
        public string OSVersion { get; set; }
        public string OSBuild { get; set; }
        public string Memory { get; set; }
        public string VirtualMemory { get; set; }
        public Dictionary<int, string> CPUs { get; set; }
    }
}
