using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HawkSync_SM.classes.SupportClasses
{
    public class SQLiteDatabaseUpdater
    {
        private string filePathExisting;
        private string filePathBackup;
        private string filePathTemplate;

        public SQLiteDatabaseUpdater(string filePathExisting, string filePathTemplate)
        {
            this.filePathExisting = filePathExisting;
            this.filePathBackup = this.filePathExisting + "_backup";
            this.filePathTemplate = filePathTemplate;
        }

        public bool RunUpdater()
        {
            if (IsTemplateVersionHigher())
            {
                if(BackupDatabase())
                {
                    try
                    {

                        Console.WriteLine("Updating database.");
                        UpdateDatabaseStructure();
                        Console.WriteLine("Database updated successfully.");

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating database structure: {ex.Message}");

                        if (RestoreDatabase())
                        {
                            MessageBox.Show("Database update failed. Restored to previous version.", "Database Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        } else
                        {
                            MessageBox.Show("Database update failed. Unable to restore to previous version.", "Database Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        
                        return false;
                    }
                } else
                {
                    return false;
                }
            }
            return true;
        }

        public bool BackupDatabase()
        {
            try
            {
                if (File.Exists(this.filePathBackup)) { File.Delete(this.filePathBackup); }
                File.Copy(this.filePathExisting, this.filePathBackup);
                Console.WriteLine($"Backup created successfully: {this.filePathBackup}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating backup: {ex.Message}");
                return false;
            }
        }

        public bool RestoreDatabase()
        {
            try
            {
                if (File.Exists(this.filePathExisting)) { File.Delete(this.filePathExisting); }

                File.Copy(this.filePathBackup, this.filePathExisting);
                Console.WriteLine($"Backup restored successfully: {this.filePathBackup}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating backup: {ex.Message}");
                return false;
            }

        }

        public bool IsTemplateVersionHigher()
        {
            int existingVersion = GetDatabaseVersion(filePathExisting);
            int templateVersion = GetDatabaseVersion(filePathTemplate);

            return templateVersion > existingVersion;
        }

        private int GetDatabaseVersion(string filePath)
        {
            string versionString = string.Empty;
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={filePath};Version=3;"))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT value FROM config WHERE key = 'db_version';", connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        versionString = reader.GetString(0);
                    }
                }
            }
            int version = Convert.ToInt32(versionString);
            return version;
        }

        public void UpdateDatabaseStructure()
        {
            using (SQLiteConnection existingConnection = new SQLiteConnection($"Data Source={filePathExisting};Version=3;"))
            using (SQLiteConnection templateConnection = new SQLiteConnection($"Data Source={filePathTemplate};Version=3;"))
            {
                existingConnection.Open();
                templateConnection.Open();

                List<string> existingTables = GetTables(existingConnection);
                List<string> templateTables = GetTables(templateConnection);

                // Drop tables in existing database that are not present in the template
                foreach (string table in existingTables)
                {
                    if (!templateTables.Contains(table))
                    {
                        DropTable(existingConnection, table);
                    }
                }

                // Add tables from template database if not present in existing database
                foreach (string table in templateTables)
                {
                    if (!existingTables.Contains(table))
                    {
                        CreateTable(existingConnection, templateConnection, table);
                    }
                }

                // Compare columns in tables and update structure
                foreach (string table in templateTables)
                {
                    List<string> existingColumns = GetColumns(existingConnection, table);
                    List<string> templateColumns = GetColumns(templateConnection, table);

                    // Drop columns not present in template
                    foreach (string column in existingColumns)
                    {
                        if (!templateColumns.Contains(column))
                        {
                            DropColumn(existingConnection, table, column);
                        }
                    }

                    // Add columns from template if not present in existing
                    foreach (string column in templateColumns)
                    {
                        if (!existingColumns.Contains(column))
                        {
                            AddColumn(existingConnection, table, column, GetColumnType(templateConnection, table, column));
                        }
                    }
                }

                Console.WriteLine("Updating Config Table...");

                List<KeyValuePair<string, string>> existingConfig = GetConfigTable(filePathExisting);
                List<KeyValuePair<string, string>> templateConfig = GetConfigTable(filePathTemplate);

                Console.WriteLine("Removing Rows...");
                foreach (var row in existingConfig)
                {
                    if (!templateConfig.Exists(r => r.Key == row.Key))
                    {
                        RemoveConfigRow(existingConnection, row.Key);
                    }
                }
                Console.WriteLine("Adding Rows...");
                // Add rows from template config table that are not present in existing
                foreach (var row in templateConfig)
                {
                    if (!existingConfig.Exists(r => r.Key == row.Key))
                    {
                        AddConfigRow(existingConnection, row.Key, row.Value);
                    }
                }

            }
        }

        private List<string> GetTables(SQLiteConnection connection)
        {
            List<string> tables = new List<string>();
            using (SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            return tables;
        }

        private List<string> GetColumns(SQLiteConnection connection, string table)
        {
            List<string> columns = new List<string>();
            using (SQLiteCommand command = new SQLiteCommand($"PRAGMA table_info({table});", connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(reader.GetString(1));
                }
            }
            return columns;
        }

        private string GetColumnType(SQLiteConnection connection, string table, string column)
        {
            using (SQLiteCommand command = new SQLiteCommand($"PRAGMA table_info({table});", connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.GetString(1) == column)
                    {
                        return reader.GetString(2);
                    }
                }
            }
            return null;
        }

        private void DropTable(SQLiteConnection connection, string table)
        {
            using (SQLiteCommand command = new SQLiteCommand($"DROP TABLE IF EXISTS {table};", connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void CreateTable(SQLiteConnection existingConnection, SQLiteConnection templateConnection, string table)
        {
            string createTableSql;
            using (SQLiteCommand command = new SQLiteCommand($"SELECT sql FROM sqlite_master WHERE type='table' AND name='{table}';", templateConnection))
            {
                createTableSql = (string)command.ExecuteScalar();
            }

            using (SQLiteCommand command = new SQLiteCommand(createTableSql, existingConnection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void DropColumn(SQLiteConnection connection, string table, string column)
        {
            using (SQLiteCommand command = new SQLiteCommand($"ALTER TABLE {table} DROP COLUMN {column};", connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void AddColumn(SQLiteConnection connection, string table, string column, string columnType)
        {
            using (SQLiteCommand command = new SQLiteCommand($"ALTER TABLE {table} ADD COLUMN {column} {columnType};", connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private List<KeyValuePair<string, string>> GetConfigTable(string filePath)
        {
            List<KeyValuePair<string, string>> config = new List<KeyValuePair<string, string>>();
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={filePath};Version=3;"))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT key, value FROM config;", connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        object keyObj = reader.GetValue(0);
                        object valueObj = reader.GetValue(1);

                        string key = keyObj != DBNull.Value ? keyObj.ToString() : null;
                        string value = valueObj != DBNull.Value ? valueObj.ToString() : null;

                        config.Add(new KeyValuePair<string, string>(key, value));
                    }
                }
            }
            return config;
        }

        private void RemoveConfigRow(SQLiteConnection connection, string key)
        {
            using (SQLiteCommand command = new SQLiteCommand($"DELETE FROM config WHERE key = @key;", connection))
            {
                command.Parameters.AddWithValue("@key", key);
                command.ExecuteNonQuery();
            }
        }

        private void AddConfigRow(SQLiteConnection connection, string key, string value)
        {
            using (SQLiteCommand command = new SQLiteCommand($"INSERT INTO config (key, value) VALUES (@key, @value);", connection))
            {
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@value", value);
                command.ExecuteNonQuery();
            }
        }

    }

}
