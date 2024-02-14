namespace HawkSync_RC.classes
{
    public class OpenClass
    {
        public enum Status
        {
            TIMEOUT = -3,
            INVALIDCOMMAND = -2,
            WELCOME = -1,
            NOTCONNECTED = 0,
            INVALIDLOGIN = 1,
            READY = 2,
            INVALIDSESSION = 3,
            LOGOUTSUCCESS = 4,
            LOGINSUCCESS = 5,
            SUCCESS = 6,
            FAILURE = 7,
            NOTAUTHENTICATED = 8,
            INVALIDINSTANCE = 9,
            INVALIDPLAYERNAME = 10,
            INVALIDIPADDRESS = 11,
            INVALIDBANREASON = 12,
            UPDATEREQUIRED = 13,
            USERALREADYEXISTS = 14
        }

        public string action { get; set; }
        public string msg { get; set; }
        public bool success { get; set; }
        public string SessionID { get; set; }
        public Status LoginMessage { get; set; }
        public string message { get; set; }
    }
}