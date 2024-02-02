namespace HawkSync_SM.classes.Plugins.pl_VoteMaps
{
    public class vm_PluginSettings
    {
        public int CoolDown { get; set; }
        public vm_internal.CoolDownTypes CoolDownType { get; set; }
        public int MinPlayers { get; set; }
        public int currentPlayers { get; private set; }
        public void setPlayers(int players)
        {
            currentPlayers = players;
        }
    }
}
