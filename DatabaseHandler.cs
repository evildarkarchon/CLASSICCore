using System.Data.SQLite;

namespace CLASSICCore
{
    public class DatabaseHandler
    {
        public static void CreateFormIdDb()
        {
            string dbPath = $"CLASSIC Data/databases/{Globals.Game} FormIDs.db";
            string txtFilePath = $"CLASSIC Data/databases/{Globals.Game} FID Main.txt";

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();

                    // Create table if not exists
                    string createTableQuery = $@"
                        CREATE TABLE IF NOT EXISTS {Globals.Game} (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,  
                            plugin TEXT, 
                            formid TEXT, 
                            entry TEXT
                        )";
                    using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Create index if not exists
                    string createIndexQuery = $"CREATE INDEX IF NOT EXISTS {Globals.Game}_index ON {Globals.Game}(formid, plugin COLLATE NOCASE)";
                    using (SQLiteCommand cmd = new SQLiteCommand(createIndexQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Read lines from text file and insert into database
                    using (StreamReader reader = new StreamReader(txtFilePath))
                    {
                        while (!reader.EndOfStream)
                        {
                            string? line = reader.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                if (line.Contains('|'))
                                {
                                    string[] parts = line.Split('|');
                                    if (parts.Length >= 3)
                                    {
                                        string plugin = parts[0].Trim();
                                        string formid = parts[1].Trim();
                                        string entry = parts[2].Trim();

                                        string insertQuery = $"INSERT INTO {Globals.Game} (plugin, formid, entry) VALUES (@plugin, @formid, @entry)";
                                        using (SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, conn))
                                        {
                                            insertCmd.Parameters.AddWithValue("@plugin", plugin);
                                            insertCmd.Parameters.AddWithValue("@formid", formid);
                                            insertCmd.Parameters.AddWithValue("@entry", entry);
                                            insertCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) // TODO: Add more specific exception handling
            {
                Console.WriteLine($"An error occurred while creating/updating the database: {ex.Message}");
                throw;
            }
        }
        public static string? GetEntry(string formid, string plugin)
        {
            string dbPath = $"CLASSIC Data/databases/{Globals.Game} FormIDs.db";

            if (File.Exists(dbPath))
            {
                using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = $"SELECT entry FROM {Globals.Game} WHERE formid=@formid AND plugin=@plugin COLLATE NOCASE";
                        cmd.Parameters.AddWithValue("@formid", formid);
                        cmd.Parameters.AddWithValue("@plugin", plugin);

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString();
                        }
                    }
                }
            }

            return null;
        }
    }
}