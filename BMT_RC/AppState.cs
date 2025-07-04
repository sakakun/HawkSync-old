﻿using System.Collections.Generic;
using HawkSync_RC.classes;

namespace HawkSync_RC
{
    public class AppState
    {
        public AppState()
        {
            Instances = new Dictionary<int, Instance>();
            IPQualityScore = new Dictionary<int, ipqualityscore>();
            CountryCodes = new List<string>();
            autoRes = new autoRestart();
            SystemInfo = new SystemInfoClass();
            ftpPort = 0;
        }

        public Dictionary<int, Instance> Instances { get; set; }
        public Dictionary<int, ipqualityscore> IPQualityScore { get; set; }
        public UserCodes UserCodes { get; set; }
        public List<string> CountryCodes { get; set; }
        public autoRestart autoRes { get; set; }
        public SystemInfoClass SystemInfo { get; set; }
        public int ftpPort { get; set; }
    }
}