using HawkSync_RC.classes;
using HawkSync_RC.classes.RCClasses;
using Newtonsoft.Json;
using Nini.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows.Forms;
using WatsonTcp;

namespace HawkSync_RC
{
    public class RCSetup
    {
        public IniConfigSource RCConfig = new IniConfigSource(ProgramConfig.RemoteINI);
        public IniConfigSource ProfileConfig = new IniConfigSource(ProgramConfig.RemoteProfiles);
        public IPAddress clientAddress { get; set; }
        public int clientPort { get; set; }
        public WatsonTcpClient client { get; set; }
        private WatsonTcpClient clientCMD { get; set; }
        public bool clientConnected { get; set; }
        public string SessionID { get; set; }
        public bool TimedOut { get; set; }
        public Timer SpectateTimer { get; set; }
        public int GamePID { get; set; }
        public IntPtr GameHandle { get; set; }
        public Dictionary<string, playerAddress> playerAddresses { get; set; }
        public int myPlayerAddress { get; set; }
        public int HostAddr { get; set; }
        public Process gameProcess { get; set; }
        public int spectateAddress { get; set; }
        public int myPlayerSlot { get; set; }
        public int tempSpectateAddress { get; set; }
        public string spectateName { get; set; }
        public OpenClass.Status clientStatus { get; set; }
        public bool loopTrigger { get; set; }
        public byte[] SendCMD(Dictionary<dynamic, dynamic> cmd)
        {
            try
            {
                clientCMD = new WatsonTcpClient(clientAddress.ToString(), clientPort);
                clientCMD.Events.StreamReceived += Events_StreamReceived;
                clientCMD.Connect();
                SyncResponse response = clientCMD.SendAndWait(3000, Compression.Compress(Encoding.Default.GetBytes(JsonConvert.SerializeObject(cmd))));
                byte[] compressResponse = Compression.Decompress(response.Data);
                clientCMD.Disconnect();
                clientCMD.Dispose();
                return compressResponse;
            }
            catch (TimeoutException)
            {
                TimedOut = true;
                return null;
            }
        }
        public byte[] SendCMD(Dictionary<string, dynamic> cmd)
        {
            try
            {
                clientCMD = new WatsonTcpClient(clientAddress.ToString(), clientPort);
                clientCMD.Events.StreamReceived += Events_StreamReceived;
                clientCMD.Connect();
                SyncResponse response = clientCMD.SendAndWait(30000, Compression.Compress(Encoding.Default.GetBytes(JsonConvert.SerializeObject(cmd))));
                byte[] compressResponse = Compression.Decompress(response.Data);
                clientCMD.Disconnect();
                clientCMD.Dispose();
                return compressResponse;
            }
            catch (TimeoutException)
            {
                FormCollection forms = Application.OpenForms;
                foreach (Form ff in forms)
                {
                    if (ff.Name != "Main_Login")
                    {
                        ff.Close();
                    }
                }
                RC_ServerManager sm = (RC_ServerManager)Application.OpenForms["ServerManager"];
                if (sm != null)
                {
                    sm.Close();
                }
                RC_Profilelist pl = (RC_Profilelist)Application.OpenForms["Profilelist"];
                if (pl != null)
                {
                    pl.Close();
                }
                MessageBox.Show("Connection lost.", "Disconnected");
                return null;
            }
        }

        private void Events_StreamReceived(object sender, StreamReceivedEventArgs e)
        {
            return;
        }
    }
}
