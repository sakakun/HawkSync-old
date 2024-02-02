using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HawkSync_RC.classes.pluginClasses;

namespace HawkSync_RC.classes
{
    public class PluginsClass
    {
        public bool WelcomeMessage { get; set; }
        public bool VoteMaps { get; set; }
        public wp_PluginSettings WelcomeMessageSettings { get; set; }
        public vm_PluginSettings VoteMapSettings { get; set; }
    }
}
