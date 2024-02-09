using HawkSync_SM.classes.Plugins.pl_VoteMaps;
using HawkSync_SM.classes.Plugins.pl_WelcomePlayer;
namespace HawkSync_SM.classes
{
    public class PluginsClass
    {
        public bool WelcomeMessage { get; set; } = false;
        public bool VoteMaps { get; set; } = false;
        public wp_PluginSettings WelcomeMessageSettings { get; set; } = new wp_PluginSettings();
        public vm_PluginSettings VoteMapSettings { get; set; } = new vm_PluginSettings();   
    }
}
