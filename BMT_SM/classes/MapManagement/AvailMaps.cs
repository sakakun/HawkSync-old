using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HawkSync_SM.classes.MapManagement
{
    internal class AvailMaps
    {

        public Dictionary<int, string> maplist = new Dictionary<int, string>();
        public Dictionary<int, MapList> availableMaps = new Dictionary<int, MapList>();
        public List<CustomMap> customMaps = new List<CustomMap>();

        public void checkAvailableMaps(AppState _state, int ArrayID)
        {

            // Clear the Current Available List Just in Case...
            _state.Instances[ArrayID].MapListAvailable.Clear();

            DirectoryInfo d = new DirectoryInfo(_state.Instances[ArrayID].profileServerPath);
            List<int> numbers = new List<int>() { 128, 64, 32, 16, 8, 4, 2, 1 };

            List<string> badMapsList = new List<string>();

            foreach (var file in d.GetFiles("*.bms"))
            {
                using (FileStream fsSourceDDS = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                using (BinaryReader binaryReader = new BinaryReader(fsSourceDDS))
                {
                    var map = new CustomMap();
                    string first_line = string.Empty;
                    try
                    {
                        first_line = File.ReadLines(_state.Instances[ArrayID].profileServerPath + "\\" + file.Name, Encoding.Default).First().ToString();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("File Name: " + file.Name + "\n" + "Reason: " + e.Message, "Skipping infoCurrentMapName File");
                        continue;
                    }

                    first_line = first_line.Replace("", "|").Replace("\0\0\0", "|");
                    string[] first_line_arr = first_line.Split("|".ToCharArray());
                    var first_line_list = new List<string>();
                    foreach (string f in first_line_arr)
                    {
                        string tmp = f.Trim().Replace("\0", ((string)"").ToString());
                        if (string.IsNullOrEmpty(tmp))
                        {
                            continue;
                        }
                        else
                        {
                            first_line_list.Add(tmp);
                        }
                    }
                    try
                    {
                        map.MapName = first_line_list[1];
                    }
                    catch
                    {
                        badMapsList.Add(file.Name);
                        continue;
                    }
                    map.FileName = file.Name;

                    fsSourceDDS.Seek(0x8A, SeekOrigin.Begin);
                    var attackdefend = binaryReader.ReadInt16();

                    if (attackdefend == 128)
                    {
                        // Set game type to be 0 (attack and defend)
                        map.gameTypeBits.Add(0);
                    }
                    else
                    {
                        fsSourceDDS.Seek(0x8B, SeekOrigin.Begin);
                        var maptype = binaryReader.ReadInt16();

                        //Console.WriteLine(file.Name);
                        // file.Name = "Backstabbers.bms"
                        _state.autoRes.sum_up(numbers, Convert.ToInt32(maptype), ref map);
                    }
                    customMaps.Add(map);
                }
            }
            SQLiteConnection db = new SQLiteConnection(ProgramConfig.dbConfig);
            db.Open();
            SQLiteCommand cmd = new SQLiteCommand("SELECT `default_maps`.`mission_name`, `default_maps`.`mission_file`, `gametypes`.`id` FROM `default_maps` INNER JOIN `gametypes` ON `default_maps`.`gametype` = `gametypes`.`shortname`;", db);
            SQLiteDataReader reader = cmd.ExecuteReader();


            while (reader.Read())
            {
                List<int> gametypes = new List<int>
                {
                    reader.GetInt32(reader.GetOrdinal("id"))
                };
                availableMaps.Add(availableMaps.Count, new MapList
                {
                    CustomMap = false,
                    MapFile = reader.GetString(reader.GetOrdinal("mission_file")),
                    MapName = reader.GetString(reader.GetOrdinal("mission_name")),
                    GameTypes = gametypes
                });
            }
            reader.Close();
            reader.Dispose();
            cmd.Dispose();
            foreach (var item in customMaps)
            {
                List<int> gametypes = new List<int>();
                foreach (var customMapBits in item.gameTypeBits)
                {
                    int gametypeId = -1;

                    // determine gametype index for each "bit" from the BMS files.
                    foreach (var gametype in _state.autoRes.gameTypes)
                    {
                        if (customMapBits == gametype.Value.Bitmap)
                        {
                            gametypeId = gametype.Value.DatabaseId;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    // EOF

                    if (gametypeId == -1)
                    {
                        throw new Exception("FUCK. WE COULDN'T DETERMINE GAME TYPE FOR MAP: " + item.FileName);
                    }
                    else
                    {
                        gametypes.Add(gametypeId);
                    }
                }
                availableMaps.Add(availableMaps.Count, new MapList
                {
                    CustomMap = true,
                    GameTypes = gametypes,
                    MapFile = item.FileName,
                    MapName = item.MapName
                });
            }

            _state.Instances[ArrayID].MapListAvailable = availableMaps;

            db.Close();
            db.Dispose();

            if (badMapsList.Count > 0)
            {
                string badMaps = string.Empty;
                foreach (string map in badMapsList)
                {
                    badMaps += map + "\n";
                }
                MessageBox.Show("Could not read map title from:\n" + badMaps + "\nThis could due to a non-converted, or a corrupted file.", "infoCurrentMapName List Error");
            }

        }

    }
}
