using HawkSync_SM.classes.Plugins.pl_VoteMaps;
using HawkSync_SM.classes.Plugins.pl_WelcomePlayer;
namespace HawkSync_SM.classes
{
    public class PluginsClass
    {
        public bool WelcomeMessage { get; set; }
        public bool VoteMaps { get; set; }
        public wp_PluginSettings WelcomeMessageSettings { get; set; }
        public vm_PluginSettings VoteMapSettings { get; set; }
    }
}
