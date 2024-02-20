using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using HawkSync_RC.classes;
using Newtonsoft.Json;
using Nini.Config;
using WatsonTcp;
using Timer = System.Windows.Forms.Timer;

namespace HawkSync_RC
{
    public partial class RC_RemoteLogin : Form
    {

        public Timer _ticker;

        private AppState _state;
        private clientClass _clientClass;
        private RC_Profilelist _profileList;
        private bool serverManagerVisible;
        private readonly RCSetup _RCSetup;
        private readonly Thread _updateInternal;

        public RC_RemoteLogin()
        {
            InitializeComponent();
            _RCSetup = new RCSetup
            {
                loopTrigger = true
            };
            // Init. Global Variables
            _state = new AppState();
            _clientClass = new clientClass();
            _ticker = new Timer();
            _ticker.Tick += Ticker_Tick;

            // Init. Server Selection
            cb_serverSelection.Items.Add("Create New Profile");
            cb_serverSelection.SelectedIndex = 0;

            
            _updateInternal = new Thread(UpdateInternal);
            _profileList = new RC_Profilelist(_state, _RCSetup);
            serverManagerVisible = false;
        }

        public void UpdateInternal()
        {
            try
            {
                while (true)
                {
                    if (_RCSetup.loopTrigger == false) break;
                    if (_RCSetup.client != null && _RCSetup.client.Connected &&
                        _RCSetup.clientStatus == OpenClass.Status.READY)
                    {
                        var Instances = _clientClass.GetInstances(_RCSetup.client, _RCSetup.SessionID);
                        if (Instances == string.Empty || _RCSetup.TimedOut) continue;
                        try
                        {
                            var instancesReply = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Instances);
                            var RCInstancesReply = new RCInstances
                            {
                                Status = (OpenClass.Status)instancesReply["Status"],
                                Instances = JsonConvert.DeserializeObject<Dictionary<int, Instance>>(
                                    Crypt.Base64Decode(instancesReply["Instances"])),
                                ChatLogs = JsonConvert.DeserializeObject<Dictionary<int, ChatLogs>>(
                                    Crypt.Base64Decode(instancesReply["ChatLogs"]))
                            };
                            if (RCInstancesReply.Status == OpenClass.Status.SUCCESS)
                            {
                                _state.Instances = RCInstancesReply.Instances;
                                _state.ChatLogs = RCInstancesReply.ChatLogs;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // ignore
            }
        }

        private void Ticker_Tick(object sender, EventArgs e)
        {
            if ((serverManagerVisible && _RCSetup.client.Connected == false) || _RCSetup.TimedOut)
            {
                serverManagerVisible = false;
                var forms = Application.OpenForms;
                foreach (Form ff in forms)
                    if (ff.Name != "Main_Login")
                        ff.Close();
                var sm = (RC_ServerManager)Application.OpenForms["ServerManager"];
                if (sm != null) sm.Close();
                var pl = (RC_Profilelist)Application.OpenForms["Profilelist"];
                if (pl != null) pl.Close();
                if (_RCSetup.TimedOut)
                {
                    connectionStatus.Text = "Timed Out";
                    connectionStatus.ForeColor = Color.FromArgb(192, 0, 0); // red
                    btn_connectServer.Text = "Connect";
                    _RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
                    _ticker.Stop();
                    _ticker.Enabled = false;
                    _clientClass = new clientClass();
                    event_toggleLogin(true);
                    Show();
                }
                else
                {
                    connectionStatus.Text = "Disconnected";
                    connectionStatus.ForeColor = Color.FromArgb(192, 0, 0); // red
                    btn_connectServer.Text = "Connect";
                    _RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
                    _ticker.Stop();
                    _ticker.Enabled = false;
                    _clientClass = new clientClass();
                    event_toggleLogin(true);
                    Show();
                }
            }
        }

        private void event_quitRemoteControl(object sender, EventArgs e)
        {
            Close();
        }

        private void event_openOptions(object sender, EventArgs e)
        {
            // client options
            var clientOptions = new ClientOptions(_state, _RCSetup);
            clientOptions.ShowDialog();
        }

        private void event_deleteProfile(object sender, EventArgs e)
        {
            // delete profile
            var profileInt = cb_serverSelection.SelectedIndex;
            _RCSetup.ProfileConfig.Configs.RemoveAt(profileInt - 1);
            _RCSetup.ProfileConfig.Save();
            cb_serverSelection.Items.RemoveAt(profileInt);
            cb_serverSelection.SelectedIndex = profileInt - 1;
            MessageBox.Show("Profile successfully deleted!", "Success");
        }

        private bool validate_profileDetails()
        {
            IPAddress address;
            if (string.IsNullOrEmpty(tb_profileName.Text) || string.IsNullOrWhiteSpace(tb_profileName.Text))
            {
                MessageBox.Show("Please enter a valid Profile Name!", "Error");
                return false;
            }

            if (string.IsNullOrEmpty(tb_hostIP.Text) || string.IsNullOrWhiteSpace(tb_hostIP.Text) ||
                !IPAddress.TryParse(tb_hostIP.Text, out address))
            {
                MessageBox.Show("Please enter a valid Hostname or IP Address!", "Error");
                return false;
            }

            if (num_hostPort.Value < 0 || num_hostPort.Value > 25565)
            {
                MessageBox.Show("Please enter a valid Port Number!", "Error");
                return false;
            }

            if (string.IsNullOrEmpty(tb_userName.Text) || string.IsNullOrWhiteSpace(tb_userName.Text))
            {
                MessageBox.Show("Please enter a valid username!", "Error");
                return false;
            }

            if (string.IsNullOrEmpty(tb_userPassword.Text) || string.IsNullOrWhiteSpace(tb_userPassword.Text))
            {
                MessageBox.Show("Please enter a valid password!", "Error");
                return false;
            }

            return true;
        }

        private void event_saveProfile(object sender, EventArgs e)
        {

            if (!validate_profileDetails()) return;

            IPAddress address;
            IPAddress.TryParse(tb_hostIP.Text, out address);

            if (cb_serverSelection.SelectedIndex == 0)
            {
                var num = cb_serverSelection.Items.Count;
                var newProfile = _RCSetup.ProfileConfig.Configs.Add(tb_profileName.Text);
                newProfile.Set("Hostname", tb_hostIP.Text);
                newProfile.Set("Port", num_hostPort.Value);
                newProfile.Set("Username", tb_userName.Text);
                newProfile.Set("Password", tb_userPassword.Text);
                _RCSetup.ProfileConfig.Save();
                cb_serverSelection.Items.Add(tb_profileName.Text);
                ProgramConfig.RCProfiles.Add(new RCProfile
                {
                    Address = address,
                    Password = tb_userPassword.Text,
                    Port = Convert.ToInt32(num_hostPort.Value),
                    ProfileName = tb_profileName.Text,
                    Username = tb_userName.Text
                });
                cb_serverSelection.SelectedIndex = num;
                MessageBox.Show("Profile Successfully Created!", "Profile Created");
            }
            else
            {
                var updateCurrentProfile =
                    _RCSetup.ProfileConfig.Configs[cb_serverSelection.Items[cb_serverSelection.SelectedIndex].ToString()];
                cb_serverSelection.Items[cb_serverSelection.SelectedIndex] = tb_profileName.Text;
                updateCurrentProfile.Name = tb_profileName.Text;
                updateCurrentProfile.Set("Hostname", tb_hostIP.Text);
                updateCurrentProfile.Set("Port", num_hostPort.Value);
                updateCurrentProfile.Set("Username", tb_userName.Text);
                updateCurrentProfile.Set("Password", tb_userPassword.Text);
                _RCSetup.ProfileConfig.Save();
                var updateProfile = ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1];
                updateProfile.Password = tb_userPassword.Text;
                updateProfile.Username = tb_userName.Text;
                updateProfile.Address = address;
                updateProfile.Port = Convert.ToInt32(num_hostPort.Value);
                updateProfile.ProfileName = tb_profileName.Text;
                MessageBox.Show("Profile Successfully Updated!", "Profile Updated");
            }
            // save profile
        }

        private void event_resetLogin()
        {
            _RCSetup.client.Dispose();
            _RCSetup.client = new WatsonTcpClient("0.0.0.0", 0);
            _RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
            _profileList = new RC_Profilelist(_state, _RCSetup);
            _RCSetup.clientAddress = IPAddress.Parse("0.0.0.0");
            
            connectionStatus.Text = "Not Connected";
            connectionStatus.ForeColor = Color.FromArgb(192, 0, 0); // red
            btn_connectServer.Text = "Connect";
            btn_launchRC.Enabled = false;

            _clientClass = new clientClass();
            _state = new AppState();
            _ticker.Stop();
            _ticker.Enabled = false;
            event_toggleLogin(true);
            
        }

        private void event_connectServer(object sender, EventArgs e)
        {
            if (!validate_profileDetails()) return;

            IPAddress address;
            IPAddress.TryParse(tb_hostIP.Text, out address);


            // If Connected then Disconnect & Reset
            if (_RCSetup.client.Connected)
            {
                var logoutStatus = _clientClass.Logout(_RCSetup.client, _RCSetup.SessionID);
                if (logoutStatus == OpenClass.Status.SUCCESS || logoutStatus == OpenClass.Status.FAILURE)
                {
                    try
                    {
                        _RCSetup.client.Disconnect();
                        event_resetLogin();
                    }
                    catch 
                    {
                        // If we can't disconnect, then we can't do anything else so just close the program
                        this.Close();
                    }
                    
                }
            }
            // If Not Connected then attempt to Connect
            else
            {
                connectionStatus.Text = "Connecting...";
                connectionStatus.ForeColor = Color.FromArgb(255, 165, 0); // orange
                // Build Connection
                var newpassword = Crypt.CreateMD5(ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1].Password);
                var username = ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1].Username;
                _RCSetup.clientAddress = ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1].Address;
                _RCSetup.clientPort = ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1].Port;
                _RCSetup.client = _clientClass.Connect(_RCSetup.clientAddress, _RCSetup.clientPort);
                // Connection Failed!!
                if (_RCSetup.client.Connected == false)
                {
                    MessageBox.Show("Failed to establish a connection!\nPlease check your hostname, and port.", "Error");
                    event_resetLogin();
                    return;
                }

                var openConn = _clientClass.Open(_RCSetup.client);
                var openConnInfo = JsonConvert.DeserializeObject<OpenClass>(openConn);

                if (openConnInfo.LoginMessage == OpenClass.Status.TIMEOUT)
                {
                    MessageBox.Show("The connection has timed out. Please try reconnecting.", "Connection Timed Out");
                    event_resetLogin();
                    return;
                }

                _RCSetup.SessionID = openConnInfo.SessionID;
                var checkLoginStatus = _clientClass.Login(_RCSetup.client, _RCSetup.SessionID, username, newpassword);
                if (checkLoginStatus == OpenClass.Status.LOGINSUCCESS)
                {
                    connectionStatus.Text = "Connected!";
                    connectionStatus.ForeColor = Color.FromArgb(0, 255, 0); // green?

                    var permissions =
                        JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(
                            _clientClass.GetUserPermissions(_RCSetup.client, _RCSetup.SessionID));
                    var RCPerms = new RCPermissions
                    {
                        Status = (OpenClass.Status)permissions["Status"],
                        UserCodes = JsonConvert.DeserializeObject<UserCodes>(
                            Crypt.Base64Decode(permissions["UserCodes"]))
                    };
                    if (RCPerms.Status == OpenClass.Status.SUCCESS)
                    {
                        _state.UserCodes = RCPerms.UserCodes;
                        Hide();
                        _RCSetup.SessionID = openConnInfo.SessionID;

                        var CountryCodes = _clientClass.GetCountryCodes(_RCSetup.client, _RCSetup.SessionID);
                        var countryCodes = JsonConvert.DeserializeObject<CountryCodes>(CountryCodes);
                        if (countryCodes.Status == OpenClass.Status.SUCCESS)
                            _state.CountryCodes = countryCodes.CountryCodeList;

                        // something went wrong... log it
                        var autoRes = _clientClass.GetAutoRes(_RCSetup.client, _RCSetup.SessionID);
                        var autoResReply = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(autoRes);
                        if ((OpenClass.Status)autoResReply["Status"] == OpenClass.Status.SUCCESS)
                            _state.autoRes =
                                JsonConvert.DeserializeObject<autoRestart>(Crypt.Base64Decode(autoResReply["AutoRes"]));

                        // something went wrong while trying to retrieve the AutoRes Class from TV
                        var ftpPortResponse = _clientClass.GetFTPPort(_RCSetup.client, _RCSetup.SessionID);
                        var ftpPortResponseObject =
                            JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ftpPortResponse);
                        if ((OpenClass.Status)ftpPortResponseObject["Status"] == OpenClass.Status.SUCCESS)
                            _state.ftpPort = (int)ftpPortResponseObject["Port"];

                        // something went wrong while trying to retrieve the AutoRes Class from TV
                        var Instances = _clientClass.GetInstances(_RCSetup.client, _RCSetup.SessionID);
                        var instancesReply = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Instances);
                        var RCInstancesReply = new RCInstances
                        {
                            Status = (OpenClass.Status)instancesReply["Status"],
                            Instances = JsonConvert.DeserializeObject<Dictionary<int, Instance>>(
                                Crypt.Base64Decode(instancesReply["Instances"])),
                            ChatLogs = JsonConvert.DeserializeObject<Dictionary<int, ChatLogs>>(
                                Crypt.Base64Decode(instancesReply["ChatLogs"]))
                        };
                        if (RCInstancesReply.Status == OpenClass.Status.SUCCESS)
                        {
                            _state.Instances = RCInstancesReply.Instances;
                            _state.ChatLogs = RCInstancesReply.ChatLogs;
                            _ticker.Enabled = true;
                            _ticker.Start();
                            btn_connectServer.Text = "Disconnect";
                            _RCSetup.clientStatus = OpenClass.Status.READY;
                            btn_launchRC.Enabled = true;
                            event_toggleLogin(false);
                            serverManagerVisible = true;
                            _profileList.ShowDialog();
                            Show();
                        }
                    }
                    // something went very wrong and we need to log it...
                }
                else if (checkLoginStatus == OpenClass.Status.INVALIDLOGIN)
                {
                    MessageBox.Show("Invalid username or password.", "Bad Credentials");
                    event_resetLogin();
                    return;
                }
                else if (checkLoginStatus == OpenClass.Status.TIMEOUT)
                {
                    MessageBox.Show("The connection has timed out. Please try reconnecting.", "Connection Timed Out");
                    event_resetLogin();
                    return;
                }

                // connect
                _RCSetup.RCConfig.Configs["Main"].Set("LastSelected", 0);
                _RCSetup.RCConfig.Save();
            }
        }

        private void Main_Login_Load(object sender, EventArgs e)
        {
            // get system info
            _RCSetup.client = new WatsonTcpClient("0.0.0.0", 0);
            _RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
            _updateInternal.Start();

            //Ticker.Interval = 10000;
            var CurrentList = _RCSetup.ProfileConfig.Configs;
            foreach (IConfig item in CurrentList)
                if (!string.IsNullOrEmpty(item.Name) || !string.IsNullOrEmpty(item.Name))
                {
                    IPAddress ipAddress;
                    var parseAddress = IPAddress.TryParse(item.Get("Hostname"), out ipAddress);
                    if (parseAddress)
                    {
                        var portNumber = -1;
                        var parsePortNumber = int.TryParse(item.Get("Port"), out portNumber);
                        if (parsePortNumber)
                            if (!string.IsNullOrEmpty(item.Get("Username")) ||
                                !string.IsNullOrWhiteSpace(item.Get("Username")))
                                if (!string.IsNullOrEmpty(item.Get("Password")) ||
                                    !string.IsNullOrWhiteSpace(item.Get("Password")))
                                {
                                    cb_serverSelection.Items.Add(item.Name);
                                    ProgramConfig.RCProfiles.Add(new RCProfile
                                    {
                                        ProfileName = item.Name,
                                        Address = ipAddress,
                                        Port = portNumber,
                                        Username = item.Get("Username"),
                                        Password = item.Get("Password")
                                    });
                                }
                    }
                }

            cb_serverSelection.SelectedIndex = 0;
        }

        private void event_serverSelectionChanged(object sender, EventArgs e)
        {
            if (cb_serverSelection.SelectedIndex == 0)
            {
                tb_profileName.Text = "";
                tb_hostIP.Text = "";
                num_hostPort.Value = 4173;
                tb_userName.Text = "";
                tb_userPassword.Text = "";
                _RCSetup.RCConfig.Configs["Main"].Set("LastSelected", 0);
            }
            else
            {
                _RCSetup.RCConfig.Configs["Main"].Set("LastSelected", cb_serverSelection.SelectedIndex);
                _RCSetup.RCConfig.Save();
                tb_profileName.Text = ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1].ProfileName;
                tb_hostIP.Text = ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1].Address.ToString();
                num_hostPort.Value = ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1].Port;
                tb_userName.Text = ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1].Username;
                tb_userPassword.Text = ProgramConfig.RCProfiles[cb_serverSelection.SelectedIndex - 1].Password;
            }
            _RCSetup.RCConfig.Save();
        }

        private void event_openServerManager(object sender, EventArgs e)
        {
            var profileList = new RC_Profilelist(_state, _RCSetup);
            Hide();
            profileList.ShowDialog();
            Show();
        }

        private void event_toggleLogin(bool yesno)
        {
            gp_loginForm.Enabled = yesno;
        }

        private void Main_Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            // close the socket and exit the program...
            if (_RCSetup.client != null && _RCSetup.client.Connected)
            {
                var logoutStatus = _clientClass.Logout(_RCSetup.client, _RCSetup.SessionID);
                if (logoutStatus == OpenClass.Status.SUCCESS || logoutStatus == OpenClass.Status.FAILURE)
                {
                    _RCSetup.client.Disconnect();
                    connectionStatus.Text = "Not Connected";
                    connectionStatus.ForeColor = Color.FromArgb(192, 0, 0); // red
                    btn_connectServer.Text = "Connect";
                    _ticker.Stop();
                    _ticker.Enabled = false;
                    _clientClass = new clientClass();
                    event_toggleLogin(true);
                }
                // something went wrong and we need to log it...
            }

            _RCSetup.loopTrigger = false;
            _updateInternal.Join();
        }

    }
}