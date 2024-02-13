using System.Net;

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
