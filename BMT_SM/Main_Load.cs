using HawkSync_SM.classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Windows.Forms;

namespace HawkSync_SM
{
    public partial class Main_Load : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        AppState state;
        Dictionary<string, byte[]> imgCache;
        Dictionary<int, ModsClass> mods;
        string registryEmail;
        string registryCode;

        public void load_modules()
        {
            Show();
            InitializeComponent();
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            string proc_text = "";
            int i = 0;
            while (i != 101)
            {
                if (i == 3)
                {
                    // Remove Later Not Needed
                    proc_text += "Checking license... ";
                    proc_text += "NOT!\n";
                    wait(1000);
                }
                if (i == 15)
                {

                    proc_text += "Loading Console Commands...";
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                    ProgramConfig.version = fileVersionInfo.FileVersion;
                    proc_text += "OK\n";

                    wait(1000);
                }
                if (i == 35)
                {

                    proc_text += "Loading Mods from DB...";
                    mods = LoadMods();
                    proc_text += "OK\n";

                    proc_text += "Loading images...";
                    imgCache = LoadImagesToMemory();
                    proc_text += "OK\n";

                    wait(1000);

                }
                if (i == 50)
                {

                    proc_text += "Determining external IP...";
                    WebClient submissionAddressClient = new WebClient()
                    {
                        BaseAddress = "https://www.myexternalip.com/"
                    };
                    string submissionAddress = submissionAddressClient.DownloadString("raw");
                    ProgramConfig.ipaddress = submissionAddress;
                    proc_text += "OK\n";

                    wait(1000);
                }
                if (i == 65)
                {

                    proc_text += "Loading Languages...OK\n";
                    LoadLanguages();

                    wait(1000);

                }
                if (i == 80)
                {

                    proc_text += "Loading BMT Configuration...";
                    GetRCPortFromDB();
                    GetWebServerPortFromDB();
                    proc_text += "OK\n";

                    wait(1000);

                }

                richTextBox1.Text = proc_text;
                progressBar1.Value = i;
                i++;

                if (i == 100)
                {
                    wait(1000);

                    this.Close();

                    state = new AppState
                    {
                        imageCache = imgCache,
                        Mods = mods,
                    };

                    Application.Run(new Main_Profilelist(state));

                }
            }
        }

        private Dictionary<string, byte[]> LoadImagesToMemory()
        {
            Dictionary<string, byte[]> img = new Dictionary<string, byte[]>();

            img.Add("bhd.gif", BitmapToByteArray(HawkSync_SM.Properties.Resources.bhd));
            img.Add("bhdts.gif", BitmapToByteArray(HawkSync_SM.Properties.Resources.bhdts));
            img.Add("notactive.gif", BitmapToByteArray(HawkSync_SM.Properties.Resources.notactive));
            img.Add("loading.gif", BitmapToByteArray(HawkSync_SM.Properties.Resources.loading));
            img.Add("hosting.gif", BitmapToByteArray(HawkSync_SM.Properties.Resources.hosting));
            img.Add("nothosting.gif", BitmapToByteArray(HawkSync_SM.Properties.Resources.nothosting));
            img.Add("scoring.gif", BitmapToByteArray(HawkSync_SM.Properties.Resources.scoring));

            return img;
        }

        static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Save the bitmap to the stream in a specific format (e.g., PNG, JPEG)
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                // Get the byte array from the memory stream
                return stream.ToArray();
            }
        }

        private byte[] imageToByteArray(string imageIn)
        {
            byte[] memory;
            FileStream fs = new FileStream(imageIn, FileMode.Open, FileAccess.Read);
            using (MemoryStream ms = new MemoryStream())
            {
                using (Image img = Image.FromStream(fs))
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                    img.Dispose();
                }
                memory = ms.ToArray();
                ms.Close();
                ms.Dispose();
            }
            fs.Close();
            fs.Dispose();
            return memory;
        }

        private void LoadLanguages()
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand langCmd = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'lang';", db);
            SQLiteDataReader langReader = langCmd.ExecuteReader();
            langReader.Read();
            ProgramConfig.Language = langReader.GetString(0);
            langReader.Close();
            db.Close();
        }

        private Dictionary<int, ModsClass> LoadMods()
        {
            Dictionary<int, ModsClass> modList = new Dictionary<int, ModsClass>();
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand ModsCmd = new SQLiteCommand("SELECT * FROM `mods`;", db);
            SQLiteDataReader ModsReader = ModsCmd.ExecuteReader();
            while (ModsReader.Read())
            {
                byte[] buffer = new byte[ModsReader.GetBytes(ModsReader.GetOrdinal("icon"), 0, null, 0, 0)];
                ModsReader.GetBytes(ModsReader.GetOrdinal("icon"), 0, buffer, 0, buffer.Length);
                modList.Add(modList.Count, new ModsClass
                {
                    ModName = ModsReader.GetString(ModsReader.GetOrdinal("name")),
                    Game = ModsReader.GetInt32(ModsReader.GetOrdinal("game")),
                    ExeArgs = ModsReader.GetString(ModsReader.GetOrdinal("args")),
                    ModIcon = buffer
                });
            }
            db.Close();
            db.Dispose();
            return modList;
        }

        private void GetRCPortFromDB()
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand port_query = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'remote_client_port';", db);
            ProgramConfig.RCPort = Convert.ToInt32(port_query.ExecuteScalar());
            port_query.Dispose();

            SQLiteCommand rcQuery = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'remote_client';", db);
            ProgramConfig.EnableRC = Convert.ToBoolean(Convert.ToInt32(rcQuery.ExecuteScalar()));
            rcQuery.Dispose();

            db.Close();
            db.Dispose();
        }

        private void GetWebServerPortFromDB()
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'web_admin_client_port';", db);
            ProgramConfig.WebServerPort = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Dispose();

            SQLiteCommand cmd2 = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'web_admin_client';", db);
            ProgramConfig.EnableWebServer = Convert.ToBoolean(Convert.ToInt32(cmd2.ExecuteScalar()));
            cmd2.Dispose();

            SQLiteCommand cmd3 = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'web_api_port';", db);
            ProgramConfig.web_api_port = Convert.ToInt32(cmd3.ExecuteScalar());
            cmd3.Dispose();

            db.Close();
            db.Dispose();
        }

        public Main_Load()
        {
            registryEmail = (string)"no@email.com";
            registryCode = (string)"1";

            load_modules();
        }

        private void wait(int time)
        {
            var timer = new System.Windows.Forms.Timer();
            if (time == 0 || time < 0) return;

            timer.Interval = time;
            timer.Enabled = true;
            timer.Start();

            timer.Tick += (s, e) =>
            {
                timer.Enabled = false;
                timer.Stop();
            };

            while (timer.Enabled)
            {
                Application.DoEvents();
            }
        }
    }
}
