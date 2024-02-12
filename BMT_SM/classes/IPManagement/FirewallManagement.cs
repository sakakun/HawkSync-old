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
            if (!bannedIPs.SequenceEqual(this.bannedIPs))
            {
                this.bannedIPs.Clear();
                this.bannedIPs.AddRange(bannedIPs);

                DeleteFirewallRules(gameName, "Deny");
                string ruleName = RulePrefix + gameName + "-Deny Traffic";

                Dictionary<string, string> suffixToMask = new Dictionary<string, string>
                {
                    { ".0.0.0", "/8" },
                    { ".0.0", "/16" },
                    { ".0", "/24" }
                };

                var badAddresses = new List<string> { "8.8.8.8" }
                    .Concat(this.bannedIPs.Select(ban =>
                    {
                        string ipAddress = ban.ipaddress;
                        var suffixMatch = suffixToMask.Keys.FirstOrDefault(suffix => ipAddress.EndsWith(suffix));
                        return suffixMatch != null ? ipAddress + suffixToMask[suffixMatch] : ipAddress;
                    }));

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
