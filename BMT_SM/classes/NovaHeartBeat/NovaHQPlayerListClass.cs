namespace HawkSync_SM.classes
{
    /*{"0": {"Name":"Host","NameBase64Encoded":"SG9zdA==","Kills":"0","Deaths":"0","WeaponId":"5","WeaponText":"CAR-15","TeamId":"5","TeamText":"None" }}*/
    public class NovaHQPlayerListClass
    {
        public string PlayerName { get; set; }
        public string NameBase64Encoded { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int WeaponId { get; set; }
        public string WeaponText { get; set; }
        public int TeamId { get; set; }
        public string TeamText { get; set; }
    }
}
