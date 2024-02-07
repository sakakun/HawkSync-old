using System;

namespace HawkSync_SM
{
    public class ipqualityClass
    {
        public int id { get; set; }
        public string address { get; set; }
        public int fraud_score { get; set; }
        public string country_code { get; set; }
        public string region { get; set; }
        public string city { get; set; }
        public string ISP { get; set; }
        public bool is_crawler { get; set; }
        public bool mobile { get; set; }
        public string host { get; set; }
        public bool proxy { get; set; }
        public bool vpn { get; set; }
        public bool tor { get; set; }
        public bool active_vpn { get; set; }
        public bool active_tor { get; set; }
        public bool recent_abuse { get; set; }
        public bool bot_status { get; set; }
        public string request_id { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public DateTime NextCheck { get; set; }
    }
}
