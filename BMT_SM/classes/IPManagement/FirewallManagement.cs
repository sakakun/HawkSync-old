using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HawkSync_SM
{
    public class FirewallManagement
    {
        private const string RulePrefix = "BHD-";
        private List<ob_playerBanList> bannedIPs = new List<ob_playerBanList>();

        public void AllowTraffic(string gameName, string serverFilePath)
        {
            DeleteFirewallRules(gameName, "Allow");
            string ruleName = RulePrefix + gameName + "-Allow Traffic";
            string script = $"New-NetFirewallRule -DisplayName \"'{ruleName}'\" -Direction Inbound -Action Allow -Program \"{serverFilePath}\"";
            RunPowerShellScript(script);
        }

        public void DenyTraffic(string gameName, string serverFilePath, List<ob_playerBanList> bannedIPs)
        {

            // Check if the contents of the bannedIPs list are the same
            if (!bannedIPs.SequenceEqual(this.bannedIPs))
            {
                // Update the List of banned IPs
                this.bannedIPs.Clear();
                this.bannedIPs.AddRange(bannedIPs);

                DeleteFirewallRules(gameName, "Deny");
                string ruleName = RulePrefix + gameName + "-Deny Traffic";

                List<string> badAddresses = new List<string>();
                badAddresses.Add("8.8.8.8");
                badAddresses.AddRange(this.bannedIPs.Select(ban => ban.ipaddress));

                string ips = string.Join(",", badAddresses);
                string script = $"New-NetFirewallRule -DisplayName \"'{ruleName}'\" -Direction Inbound -Action Block -Program \"{serverFilePath}\" -RemoteAddress \"{ips}\"";

                RunPowerShellScript(script);
            }
  
            

        }

        public void DeleteFirewallRules(string gameName, string trafficControlType)
        {
            string rulePrefix = RulePrefix + gameName + "-" + trafficControlType + " Traffic";
            string script = $"Get-NetFirewallRule -DisplayName \"'{rulePrefix}'\" | Remove-NetFirewallRule";
            RunPowerShellScript(script);
        }

        private bool RunPowerShellScript(string script)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy unrestricted -Command \"{script}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                if (process != null)
                {
                    // No need to wait for the process to exit
                    return true;
                }
                else
                {
                    // Log an error or return false
                    return false; // Failed to start the process
                }
            }
        }

    }
}
