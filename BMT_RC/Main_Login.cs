using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nini.Config;
using Timer = System.Windows.Forms.Timer;
using HawkSync_RC.classes;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.Threading;
using WatsonTcp;
using System.Management;
using System.Runtime.InteropServices;

namespace HawkSync_RC
{
    public partial class Main_Login : Form
    {
        RCSetup RCSetup;
        AppState _state;
        public Timer Ticker;
        clientClass clientClass;
        Thread updateInternal;
        RC_Profilelist profileList;
        bool openedProfileList;
        //Timer updateInternal;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        public Main_Login()
        {
            InitializeComponent();
            RCSetup = new RCSetup
            {
                loopTrigger = true
            };
            _state = new AppState();
            clientClass = new clientClass();
            Ticker = new Timer();
            //updateInternal = new Timer();
            textBox3.PasswordChar = '*';
            comboBox1.Items.Add("Create New Profile");
            comboBox1.SelectedIndex = 0;
            //Ticker.Interval = 1000;
            Ticker.Tick += Ticker_Tick;
            /*updateInternal.Tick += UpdateInternal_Tick;
            updateInternal.Interval = 100;*/
            updateInternal = new Thread(UpdateInternal);
            profileList = new RC_Profilelist(_state, RCSetup);
            openedProfileList = false;
        }

        private void UpdateInternal_Tick(object sender, EventArgs e)
        {
            if (RCSetup.loopTrigger == false)
            {
                return;
            }
            if ((RCSetup.client != null && RCSetup.client.Connected == true) && (RCSetup.clientStatus == OpenClass.Status.READY))
            {
                string Instances = clientClass.GetInstances(RCSetup.client, RCSetup.SessionID);
                if (Instances == string.Empty || RCSetup.TimedOut == true)
                {
                    return;
                }
                try
                {
                    Dictionary<dynamic, dynamic> instancesReply = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Instances);
                    RCInstances RCInstancesReply = new RCInstances
                    {
                        Status = (OpenClass.Status)instancesReply["Status"],
                        Instances = JsonConvert.DeserializeObject<Dictionary<int, Instance>>(Crypt.Base64Decode(instancesReply["Instances"])),
                        ChatLogs = JsonConvert.DeserializeObject<Dictionary<int, ChatLogs>>(Crypt.Base64Decode(instancesReply["ChatLogs"]))
                    };
                    if (RCInstancesReply.Status == OpenClass.Status.SUCCESS)
                    {
                        _state.Instances = RCInstancesReply.Instances;
                        _state.ChatLogs = RCInstancesReply.ChatLogs;
                    }
                }
                catch
                {
                    return;
                }
            }
        }
        public void UpdateInternal()
        {
            try
            {
                while (true)
                {
                    if (RCSetup.loopTrigger == false)
                    {
                        break;
                    }
                    if ((RCSetup.client != null && RCSetup.client.Connected == true) && (RCSetup.clientStatus == OpenClass.Status.READY))
                    {
                        string Instances = clientClass.GetInstances(RCSetup.client, RCSetup.SessionID);
                        if (Instances == string.Empty || RCSetup.TimedOut == true)
                        {
                            continue;
                        }
                        try
                        {
                            Dictionary<dynamic, dynamic> instancesReply = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Instances);
                            RCInstances RCInstancesReply = new RCInstances
                            {
                                Status = (OpenClass.Status)instancesReply["Status"],
                                Instances = JsonConvert.DeserializeObject<Dictionary<int, Instance>>(Crypt.Base64Decode(instancesReply["Instances"])),
                                ChatLogs = JsonConvert.DeserializeObject<Dictionary<int, ChatLogs>>(Crypt.Base64Decode(instancesReply["ChatLogs"]))
                            };
                            if (RCInstancesReply.Status == OpenClass.Status.SUCCESS)
                            {
                                _state.Instances = RCInstancesReply.Instances;
                                _state.ChatLogs = RCInstancesReply.ChatLogs;
                            }
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                // ignore
            }
        }

        private void Ticker_Tick(object sender, EventArgs e)
        {
            //if (updateInternal.ThreadState == System.Threading.ThreadState.Stopped || updateInternal.ThreadState == System.Threading.ThreadState.Aborted || updateInternal.ThreadState == System.Threading.ThreadState.Background || updateInternal.ThreadState == System.Threading.ThreadState.Unstarted)
            //{
            //updateInternal = new Thread(UpdateInternal);
            //updateInternal.Start();
            //}
            /*new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                UpdateInternal();
            }).Start();*/
            if (openedProfileList == true && RCSetup.client.Connected == false || RCSetup.TimedOut == true)
            {
                openedProfileList = false;
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
                if (RCSetup.TimedOut == true)
                {
                    label8.Text = "Timed Out";
                    label8.ForeColor = Color.FromArgb(192, 0, 0); // red
                    button3.Text = "Connect";
                    RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
                    Ticker.Stop();
                    Ticker.Enabled = false;
                    clientClass = new clientClass();
                    Options(true);
                    this.Show();
                }
                else
                {
                    label8.Text = "Disconnected";
                    label8.ForeColor = Color.FromArgb(192, 0, 0); // red
                    button3.Text = "Connect";
                    RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
                    Ticker.Stop();
                    Ticker.Enabled = false;
                    clientClass = new clientClass();
                    Options(true);
                    this.Show();
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // check for updates
            Process process = new Process();
            process.StartInfo.FileName = "BMTRCUpdater.exe";
            process.StartInfo.Arguments = "/justcheck";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
            var exitcode = process.ExitCode;
            if (exitcode == -536870895)
            {
                // no update
                MessageBox.Show("No new updates available.", "Check For Updates");
            }
            else
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.Arguments = "";
                process.Start();
            }
            process.Dispose();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            // client options
            ClientOptions clientOptions = new ClientOptions(_state, RCSetup);
            clientOptions.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // delete profile
            int profileInt = comboBox1.SelectedIndex;
            RCSetup.ProfileConfig.Configs.RemoveAt(profileInt-1);
            RCSetup.ProfileConfig.Save();
            comboBox1.Items.RemoveAt(profileInt);
            comboBox1.SelectedIndex = profileInt-1;
            MessageBox.Show("Profile successfully deleted!", "Success");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPAddress address;
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid Profile Name!", "Error");
                return;
            }
            else if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || !IPAddress.TryParse(textBox2.Text, out address))
            {
                MessageBox.Show("Please enter a valid Hostname or IP Address!", "Error");
                return;
            }
            else if (numericUpDown1.Value < 0 || numericUpDown1.Value > 25565)
            {
                MessageBox.Show("Please enter a valid Port Number!", "Error");
                return;
            }
            else if (string.IsNullOrEmpty(textBox4.Text) || string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Please enter a valid username!", "Error");
                return;
            }
            else if (string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Please enter a valid password!", "Error");
                return;
            }
            if (comboBox1.SelectedIndex == 0)
            {
                int num = comboBox1.Items.Count;
                IConfig newProfile = RCSetup.ProfileConfig.Configs.Add(textBox1.Text);
                newProfile.Set("Hostname", textBox2.Text);
                newProfile.Set("Port", numericUpDown1.Value);
                newProfile.Set("Username", textBox4.Text);
                newProfile.Set("Password", textBox3.Text);
                RCSetup.ProfileConfig.Save();
                comboBox1.Items.Add(textBox1.Text);
                ProgramConfig.RCProfiles.Add(new RCProfile
                {
                    Address = address,
                    Password = textBox3.Text,
                    Port = Convert.ToInt32(numericUpDown1.Value),
                    ProfileName = textBox1.Text,
                    Username = textBox4.Text
                });
                comboBox1.SelectedIndex = num;
                MessageBox.Show("Profile Successfully Created!", "Profile Created");
            }
            else
            {
                IConfig updateCurrentProfile = RCSetup.ProfileConfig.Configs[comboBox1.Items[comboBox1.SelectedIndex].ToString()];
                comboBox1.Items[comboBox1.SelectedIndex] = textBox1.Text;
                updateCurrentProfile.Name = textBox1.Text;
                updateCurrentProfile.Set("Hostname", textBox2.Text);
                updateCurrentProfile.Set("Port", numericUpDown1.Value);
                updateCurrentProfile.Set("Username", textBox4.Text);
                updateCurrentProfile.Set("Password", textBox3.Text);
                RCSetup.ProfileConfig.Save();
                RCProfile updateProfile = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1];
                updateProfile.Password = textBox3.Text;
                updateProfile.Username = textBox4.Text;
                updateProfile.Address = address;
                updateProfile.Port = Convert.ToInt32(numericUpDown1.Value);
                updateProfile.ProfileName = textBox1.Text;
                MessageBox.Show("Profile Successfully Updated!", "Profile Updated");
            }
            // save profile
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IPAddress address;
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid Profile Name!", "Error");
                return;
            }
            else if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || !IPAddress.TryParse(textBox2.Text, out address))
            {
                MessageBox.Show("Please enter a valid Hostname or IP Address!", "Error");
                return;
            }
            else if (numericUpDown1.Value < 0 || numericUpDown1.Value > 25565)
            {
                MessageBox.Show("Please enter a valid Port Number!", "Error");
                return;
            }
            else if (string.IsNullOrEmpty(textBox4.Text) || string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("Please enter a valid username!", "Error");
                return;
            }
            else if (string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Please enter a valid password!", "Error");
                return;
            }
            if (comboBox1.SelectedIndex == 0 && !RCSetup.client.Connected)
            {
                int num = comboBox1.Items.Count;
                IConfig newProfile = RCSetup.ProfileConfig.Configs.Add(textBox1.Text);
                newProfile.Set("Hostname", textBox2.Text);
                newProfile.Set("Port", numericUpDown1.Value);
                newProfile.Set("Username", textBox4.Text);
                newProfile.Set("Password", textBox3.Text);
                ProgramConfig.RCProfiles.Add(new RCProfile
                {
                    ProfileName = textBox1.Text,
                    Address = address,
                    Port = Convert.ToInt32(numericUpDown1.Value),
                    Username = textBox4.Text,
                    Password = textBox3.Text
                });
                RCSetup.ProfileConfig.Save();
                comboBox1.Items.Add(textBox1.Text);
                comboBox1.SelectedIndex = num;
                MessageBox.Show("Profile Successfully Created!", "Profile Created");
            }
            else if (comboBox1.SelectedIndex != 0 && !RCSetup.client.Connected)
            {
                IConfig updateCurrentProfile = RCSetup.ProfileConfig.Configs[comboBox1.Items[comboBox1.SelectedIndex].ToString()];
                comboBox1.Items[comboBox1.SelectedIndex] = textBox1.Text;
                updateCurrentProfile.Name = textBox1.Text;
                updateCurrentProfile.Set("Hostname", textBox2.Text);
                updateCurrentProfile.Set("Port", numericUpDown1.Value);
                updateCurrentProfile.Set("Username", textBox4.Text);
                updateCurrentProfile.Set("Password", textBox3.Text);
                RCSetup.ProfileConfig.Save();
                RCProfile updateProfile = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1];
                updateProfile.ProfileName = textBox1.Text;
                updateProfile.Address = address;
                updateProfile.Port = Convert.ToInt32(numericUpDown1.Value);
                updateProfile.Username = textBox4.Text;
                updateProfile.Password = textBox3.Text;
            }
            if (RCSetup.client.Connected)
            {
                OpenClass.Status logoutStatus = clientClass.Logout(RCSetup.client, RCSetup.SessionID);
                if (logoutStatus == OpenClass.Status.SUCCESS || logoutStatus == OpenClass.Status.FAILURE)
                {
                    try
                    {
                        RCSetup.client.Disconnect();
                    }
                    catch
                    {
                        // ignored
                    }
                    RCSetup.client.Dispose();
                    RCSetup.client = new WatsonTcpClient("0.0.0.0", 0);
                    RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
                    label8.Text = "Not Connected";
                    label8.ForeColor = Color.FromArgb(192, 0, 0); // red
                    button3.Text = "Connect";
                    Ticker.Stop();
                    Ticker.Enabled = false;
                    Options(true);
                    clientClass = new clientClass();
                    _state = new AppState(); // added to prevent data overflow from switching between TV clients
                    profileList = new RC_Profilelist(_state, RCSetup);
                    RCSetup.clientAddress = IPAddress.Parse("0.0.0.0");
                    button12.Enabled = false;
                }
                else
                {
                    // something went wrong and we need to log it...
                }
            }
            else
            {
                label8.Text = "Connecting...";
                label8.ForeColor = Color.FromArgb(255, 165, 0); // orange
                string newpassword = Crypt.CreateMD5(ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Password);
                string username = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Username;
                RCSetup.client = clientClass.Connect(ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Address, ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Port);
                RCSetup.clientAddress = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Address;
                RCSetup.clientPort = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Port;
                if (RCSetup.client.Connected == false)
                {
                    MessageBox.Show("Failed to establish a connection!\nPlease check your hostname, and port.", "Error");
                    label8.Text = "Not Connected";
                    label8.ForeColor = Color.FromArgb(192, 0, 0); // red
                    return;
                }

                string openConn = clientClass.Open(RCSetup.client);
                
                OpenClass openConnInfo = JsonConvert.DeserializeObject<OpenClass>(openConn);

                if (openConnInfo.LoginMessage == OpenClass.Status.UPDATEREQUIRED)
                {
                    label8.Text = "Disconnected";
                    label8.ForeColor = Color.Red;
                    RCSetup.client.Disconnect();
                    button3.Text = "Connect";
                    RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
                    Ticker.Stop();
                    Ticker.Enabled = false;
                    clientClass = new clientClass();
                    RCSetup.clientAddress = IPAddress.Parse("0.0.0.0");
                    button12.Enabled = false;
                    MessageBox.Show("The BMT TV is newer than the RC.\nPlease check for updates.", "Update Required");
                    return;
                }
                if (openConnInfo.LoginMessage == OpenClass.Status.TIMEOUT)
                {
                    label8.Text = "Timed Out";
                    label8.ForeColor = Color.Red;
                    RCSetup.client.Disconnect();
                    button3.Text = "Connect";
                    RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
                    Ticker.Stop();
                    Ticker.Enabled = false;
                    clientClass = new clientClass();
                    RCSetup.clientAddress = IPAddress.Parse("0.0.0.0");
                    button12.Enabled = false;
                    MessageBox.Show("The connection has timed out. Please try reconnecting.", "Connection Timed Out");
                    return;
                }
                RCSetup.SessionID = openConnInfo.SessionID;
                OpenClass.Status checkLoginStatus = clientClass.Login(RCSetup.client, RCSetup.SessionID, username, newpassword);
                if (checkLoginStatus == OpenClass.Status.LOGINSUCCESS)
                {
                    label8.Text = "Connected!";
                    label8.ForeColor = Color.FromArgb(0, 255, 0); // green?

                    Dictionary<dynamic, dynamic> permissions = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(clientClass.GetUserPermissions(RCSetup.client, RCSetup.SessionID));
                    RCPermissions RCPerms = new RCPermissions
                    {
                        Status = (OpenClass.Status)permissions["Status"],
                        UserCodes = JsonConvert.DeserializeObject<UserCodes>(Crypt.Base64Decode(permissions["UserCodes"]))
                    };
                    if (RCPerms.Status == OpenClass.Status.SUCCESS)
                    {
                        _state.UserCodes = RCPerms.UserCodes;
                        this.Hide();
                        RCSetup.SessionID = openConnInfo.SessionID;

                        string CountryCodes = clientClass.GetCountryCodes(RCSetup.client, RCSetup.SessionID);
                        CountryCodes countryCodes = JsonConvert.DeserializeObject<CountryCodes>(CountryCodes);
                        if (countryCodes.Status == OpenClass.Status.SUCCESS)
                        {
                            _state.CountryCodes = countryCodes.CountryCodeList;
                        }
                        else
                        {
                            // something went wrong... log it
                        }

                        string autoRes = clientClass.GetAutoRes(RCSetup.client, RCSetup.SessionID);
                        Dictionary<string, dynamic> autoResReply = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(autoRes);
                        if ((OpenClass.Status)autoResReply["Status"] == OpenClass.Status.SUCCESS)
                        {
                            _state.autoRes = JsonConvert.DeserializeObject<autoRes>(Crypt.Base64Decode(autoResReply["AutoRes"]));
                        }
                        else
                        {
                            // something went wrong while trying to retrieve the AutoRes Class from TV
                        }

                        string ftpPortResponse = clientClass.GetFTPPort(RCSetup.client, RCSetup.SessionID);
                        Dictionary<string, dynamic> ftpPortResponseObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(ftpPortResponse);
                        if ((OpenClass.Status)ftpPortResponseObject["Status"] == OpenClass.Status.SUCCESS)
                        {
                            _state.ftpPort = (int)ftpPortResponseObject["Port"];
                        }
                        else
                        {
                            // something went wrong while trying to retrieve the AutoRes Class from TV
                        }

                        string Instances = clientClass.GetInstances(RCSetup.client, RCSetup.SessionID);
                        Dictionary<dynamic, dynamic> instancesReply = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(Instances);
                        RCInstances RCInstancesReply = new RCInstances
                        {
                            Status = (OpenClass.Status)instancesReply["Status"],
                            Instances = JsonConvert.DeserializeObject<Dictionary<int, Instance>>(Crypt.Base64Decode(instancesReply["Instances"])),
                            ChatLogs = JsonConvert.DeserializeObject<Dictionary<int, ChatLogs>>(Crypt.Base64Decode(instancesReply["ChatLogs"]))
                        };
                        if (RCInstancesReply.Status == OpenClass.Status.SUCCESS)
                        {
                            _state.Instances = RCInstancesReply.Instances;
                            _state.ChatLogs = RCInstancesReply.ChatLogs;
                            Ticker.Enabled = true;
                            Ticker.Start();
                            button3.Text = "Disconnect";
                            RCSetup.clientStatus = OpenClass.Status.READY;
                            button12.Enabled = true;
                            Options(false);
                            openedProfileList = true;
                            profileList.ShowDialog();
                            this.Show();
                        }
                    }
                    else
                    {
                        // something went very wrong and we need to log it...
                    }
                }
                else if (checkLoginStatus == OpenClass.Status.INVALIDLOGIN)
                {
                    MessageBox.Show("Invalid username or password.", "Bad Credentials");
                    label8.Text = "Not Connected";
                    label8.ForeColor = Color.FromArgb(192, 0, 0); // red
                    return;
                }
                else if (checkLoginStatus == OpenClass.Status.TIMEOUT)
                {

                }
                // connect
                RCSetup.RCConfig.Configs["Main"].Set("LastSelected", comboBox1.SelectedIndex);
                RCSetup.RCConfig.Save();
            }
        }
        private SystemInfoClass GatherSystemInfo()
        {
            SystemInfoClass systemInfo = new SystemInfoClass();
            ManagementObjectSearcher MOS = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject MOS_info in MOS.Get())
            {
                systemInfo.OSName = (string)MOS_info["Caption"];
                systemInfo.OSVersion = (string)MOS_info["Version"];
                systemInfo.OSBuild = Environment.OSVersion.Version.Build.ToString();
                systemInfo.VirtualMemory = Convert.ToString((Convert.ToInt32(MOS_info["TotalVirtualMemorySize"])) / 1024 / 1024) + "GB";
            }
            ManagementObjectSearcher CPU = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            systemInfo.CPUs = new Dictionary<int, string>();
            foreach (ManagementObject CPU_info in CPU.Get())
            {
                systemInfo.CPUs.Add(systemInfo.CPUs.Count, (string)CPU_info["Name"]);
            }
            long memkb;
            GetPhysicallyInstalledSystemMemory(out memkb);
            systemInfo.Memory = Convert.ToString((memkb / 1024 / 1024) + "GB");

            return systemInfo;
        }
        private void Main_Login_Load(object sender, EventArgs e)
        {
            // get system info
            _state.SystemInfo = GatherSystemInfo();
            RCSetup.client = new WatsonTcpClient("0.0.0.0", 0);
            RCSetup.clientStatus = OpenClass.Status.NOTCONNECTED;
            updateInternal.Start();

            //Ticker.Interval = 10000;
            ConfigCollection CurrentList = RCSetup.ProfileConfig.Configs;
            foreach (IConfig item in CurrentList)
            {
                if (!string.IsNullOrEmpty(item.Name) || !string.IsNullOrEmpty(item.Name))
                {
                    IPAddress ipAddress;
                    bool parseAddress = IPAddress.TryParse(item.Get("Hostname"), out ipAddress);
                    if (parseAddress == true)
                    {
                        int portNumber = -1;
                        bool parsePortNumber = int.TryParse(item.Get("Port"), out portNumber);
                        if (parsePortNumber == true)
                        {
                            if (!string.IsNullOrEmpty(item.Get("Username")) || !string.IsNullOrWhiteSpace(item.Get("Username")))
                            {
                                if (!string.IsNullOrEmpty(item.Get("Password")) || !string.IsNullOrWhiteSpace(item.Get("Password")))
                                {
                                    comboBox1.Items.Add(item.Name);
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
                    }
                }
            }
            comboBox1.SelectedIndex = RCSetup.RCConfig.Configs["Main"].GetInt("LastSelected");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                textBox1.Text = "";
                textBox2.Text = "";
                numericUpDown1.Value = 4173;
                textBox4.Text = "";
                textBox3.Text = "";
            }
            else
            {
                RCSetup.RCConfig.Configs["Main"].Set("LastSelected", comboBox1.SelectedIndex);
                RCSetup.RCConfig.Save();
                textBox1.Text = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].ProfileName;
                textBox2.Text = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Address.ToString();
                numericUpDown1.Value = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Port;
                textBox4.Text = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Username;
                textBox3.Text = ProgramConfig.RCProfiles[comboBox1.SelectedIndex - 1].Password;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            RC_Profilelist profileList = new RC_Profilelist(_state, RCSetup);
            this.Hide();
            profileList.ShowDialog();
            this.Show();
        }
        private void Options(bool yesno)
        {
            comboBox1.Enabled = yesno;
            textBox1.Enabled = yesno;
            textBox2.Enabled = yesno;
            textBox3.Enabled = yesno;
            textBox4.Enabled = yesno;
            button1.Enabled = yesno;
            button2.Enabled = yesno;
            button11.Enabled = yesno;
            numericUpDown1.Enabled = yesno;
        }


        private void Main_Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            // close the socket and exit the program...
            if (RCSetup.client != null && RCSetup.client.Connected == true)
            {
                OpenClass.Status logoutStatus = clientClass.Logout(RCSetup.client, RCSetup.SessionID);
                if (logoutStatus == OpenClass.Status.SUCCESS || logoutStatus == OpenClass.Status.FAILURE)
                {
                    RCSetup.client.Disconnect();
                    label8.Text = "Not Connected";
                    label8.ForeColor = Color.FromArgb(192, 0, 0); // red
                    button3.Text = "Connect";
                    Ticker.Stop();
                    /*updateInternal.Stop();
                    updateInternal.Enabled = false;*/
                    Ticker.Enabled = false;
                    clientClass = new clientClass();
                    Options(true);
                }
                else
                {
                    // something went wrong and we need to log it...
                }
            }
            RCSetup.loopTrigger = false;
            updateInternal.Abort();
        }
    }
}
