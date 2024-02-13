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
    public partial class Main_Load
    {

        public Main_Load()
        {
            AppState state;

            state = new AppState
            {
                imageCache = LoadImagesToMemory(),
                Mods = LoadMods(),
            };

            Application.Run(new SM_ProfileList(state));

        }

        // Convert a bitmap to a byte array used in LoadImagesToMemory
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

    }
}
