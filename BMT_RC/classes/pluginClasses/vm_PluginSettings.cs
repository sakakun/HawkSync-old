using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_RC.classes.pluginClasses
{
    public class vm_PluginSettings
    {
        public int CoolDown { get; set; }
        public vm_internal.CoolDownTypes CoolDownType { get; set; }
        public int MinPlayers { get; set; }
    }
}
