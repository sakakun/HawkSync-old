namespace HawkSync_SM.classes
{
    public class Queue
    {
        public ConsoleQueueType Type { get; set; }
        public string text { get; set; }
        public ChatColor color { get; set; }
    }
    public enum ConsoleQueueType
    {
        MESSAGE,
        PLAYER_WARNING,
        CMD_SCOREMAP,
        CMD_KICKPLAYER
    }
    public enum ChatColor
    {
        NONE,
        MOD_WHITE,
        YELLOW,
        NORMAL,
        TEAM_BLUE,
        TEAM_RED
    }
}
