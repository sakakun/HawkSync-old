using System.Data.SQLite;

namespace HawkSync_SM
{
    public class sqlite
    {
        public string db = "settings.sqlite";
        public string ExecuteNonQuery(string sql)
        {
            using (SQLiteConnection cnn = new SQLiteConnection(ProgramConfig.DBConfig))
            using (SQLiteCommand mycommand = new SQLiteCommand(sql, cnn))
            {
                cnn.Open();
                object value = mycommand.ExecuteNonQuery();
                return (value != null ? value.ToString() : "");
            }
        }
        public SQLiteDataReader ExecuteReader(string sql)
        {
            using (SQLiteConnection cnn = new SQLiteConnection(ProgramConfig.DBConfig))
            using (SQLiteCommand mycommand = new SQLiteCommand(sql, cnn))
            {

                return mycommand.ExecuteReader();
            }
        }
    }
}
