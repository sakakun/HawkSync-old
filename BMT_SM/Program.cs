using HawkSync_SM.classes;
using HawkSync_SM.classes.SupportClasses;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace HawkSync_SM
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.3", false);
            AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.2", false);
            AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures", false);

            // Load Remote Control Configurations
            GetRemoteControlConfig(); 

            // Application Version Statements
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            ProgramConfig.ApplicationVersion = fileVersionInfo.FileVersion;
            ProgramConfig.ApplicationState = new AppState
            {
                imageCache = LoadImagesToMemory(),
                Mods = LoadMods(),
            };

            // Start the Application (Main Profile List)
            Application.Run(new SM_ProfileList(ProgramConfig.ApplicationState));

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

        // Load images to memory
        static private Dictionary<string, byte[]> LoadImagesToMemory()
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

        // Load mods from the database
        static private Dictionary<int, ModsClass> LoadMods()
        {
            Dictionary<int, ModsClass> modList = new Dictionary<int, ModsClass>();
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
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

        static private void GetRemoteControlConfig()
        {
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
            db.Open();
            SQLiteCommand port_query = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'remote_client_port';", db);
            ProgramConfig.RCPort = Convert.ToInt32(port_query.ExecuteScalar());
            port_query.Dispose();

            SQLiteCommand rcQuery = new SQLiteCommand("SELECT `value` FROM `config` WHERE `key` = 'remote_client';", db);
            ProgramConfig.RCEnabled = Convert.ToBoolean(Convert.ToInt32(rcQuery.ExecuteScalar()));
            rcQuery.Dispose();

            db.Close();
            db.Dispose();
        }

    }
}
