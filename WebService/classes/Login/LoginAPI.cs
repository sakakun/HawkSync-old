using WebService.classes.User;

namespace WebService.classes.Login
{
    public class LoginAPI
    {
        public bool success { get; set; }
        public string error { get; set; }
        public int userid { get; set; }
        public bool superadmin { get; set; }
        public int subadmin { get; set; }
        public UserCodes permissions { get; set; }
    }
}