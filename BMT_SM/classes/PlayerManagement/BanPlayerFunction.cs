using System;
using System.Data.SQLite;

namespace HawkSync_SM
{
    public class BanPlayerFunction
    {
        public void BanPlayer(Instance _instances, int slotNum, string banLimit, string bannedBy, string banReason, bool onlyKick = false)
        {
            if (onlyKick == false)
            {
                if (banLimit == "-1")
                {
                    // perm ban
                    _instances.BanList.Add(new playerbans
                    {
                        expires = banLimit,
                        id = _instances.BanList.Count,
                        ipaddress = _instances.PlayerList[slotNum].address,
                        lastseen = DateTime.Now,
                        newBan = true,
                        player = _instances.PlayerList[slotNum].name,
                        reason = banReason,
                        retry = DateTime.Now,
                        onlykick = onlyKick,
                        VPNBan = false,
                        addedDate = DateTime.Now,
                        bannedBy = bannedBy
                    });
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand lastID_query = new SQLiteCommand("SELECT `id` FROM `playerbans` ORDER BY `id` DESC LIMIT 1;", db);
                    int NextID = Convert.ToInt32(lastID_query.ExecuteScalar());
                    NextID++; // +1 for the NEXT ID
                    lastID_query.Dispose();
                    SQLiteCommand query = new SQLiteCommand("INSERT INTO `playerbans` (`id`, `profileid`, `player`, `PublicIP`, `dateadded`, `lastseen`, `reason`, `expires`, `bannedby`) VALUES (@newid, @profileid, @playername, @playerip, @dateadded, @date, @reason, @expires, @bannedby);", db);
                    query.Parameters.AddWithValue("@newid", NextID);
                    query.Parameters.AddWithValue("@profileid", _instances.Id);
                    query.Parameters.AddWithValue("@playername", _instances.PlayerList[slotNum].name);
                    query.Parameters.AddWithValue("@playerip", _instances.PlayerList[slotNum].address);
                    query.Parameters.AddWithValue("@dateadded", DateTime.Now);
                    query.Parameters.AddWithValue("@date", DateTime.Now);
                    query.Parameters.AddWithValue("@reason", banReason);
                    query.Parameters.AddWithValue("@expires", "-1");
                    query.Parameters.AddWithValue("@bannedby", bannedBy);
                    query.ExecuteNonQuery();
                    query.Dispose();
                    db.Close();
                    db.Dispose();
                }
                else
                {
                    SQLiteConnection db = new SQLiteConnection(ProgramConfig.DBConfig);
                    db.Open();
                    SQLiteCommand lastID_query = new SQLiteCommand("SELECT `id` FROM `playerbans` ORDER BY `id` DESC LIMIT 1;", db);
                    int NextID = Convert.ToInt32(lastID_query.ExecuteScalar());
                    NextID++; // +1 for the NEXT ID
                    _instances.BanList.Add(new playerbans
                    {
                        expires = DateTime.Parse(banLimit).ToString(),
                        id = NextID,
                        ipaddress = _instances.PlayerList[slotNum].address,
                        lastseen = DateTime.Now,
                        newBan = false,
                        player = _instances.PlayerList[slotNum].name,
                        reason = banReason,
                        retry = DateTime.Now,
                        onlykick = onlyKick,
                        VPNBan = false,
                        addedDate = DateTime.Now,
                        bannedBy = bannedBy
                    });
                    SQLiteCommand query = new SQLiteCommand("INSERT INTO `playerbans` (`id`, `profileid`, `player`, `PublicIP`, `dateadded`, `lastseen`, `reason`, `expires`, `bannedby`) VALUES (@newid, @profileid, @playername, @playerip, @dateadded, @date, @reason, @expires, @bannedby);", db);
                    query.Parameters.AddWithValue("@newid", NextID);
                    query.Parameters.AddWithValue("@profileid", _instances.Id);
                    query.Parameters.AddWithValue("@playername", _instances.PlayerList[slotNum].name);
                    query.Parameters.AddWithValue("@playerip", _instances.PlayerList[slotNum].address);
                    query.Parameters.AddWithValue("@date", DateTime.Now);
                    query.Parameters.AddWithValue("@dateadded", DateTime.Now);
                    query.Parameters.AddWithValue("@reason", banReason);
                    query.Parameters.AddWithValue("@expires", DateTime.Parse(banLimit));
                    query.Parameters.AddWithValue("@bannedby", bannedBy);
                    query.ExecuteNonQuery();
                    query.Dispose();
                    db.Close();
                    db.Dispose();
                }
            }
            else
            {
                _instances.BanList.Add(new playerbans
                {
                    expires = DateTime.Parse(banLimit).ToString(),
                    id = _instances.BanList.Count,
                    ipaddress = _instances.PlayerList[slotNum].address,
                    lastseen = DateTime.Now,
                    newBan = true,
                    player = _instances.PlayerList[slotNum].name,
                    reason = banReason,
                    retry = DateTime.Now,
                    onlykick = onlyKick,
                    VPNBan = false,
                    addedDate = DateTime.Now,
                    bannedBy = bannedBy
                });
            }
        }
    }
}
