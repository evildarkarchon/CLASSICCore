using System.IO.Compression;

namespace CLASSICCore
{
    public class DataExtractor
    {
        public static void ClassicDataExtract()
        {
            string zipPath = "CLASSIC Data/CLASSIC Data.zip";
            string fallbackZipPath = "CLASSIC Data.zip";
            string extractPath = "CLASSIC Data";
            string mainYamlPath = "CLASSIC Data/databases/CLASSIC Main.yaml";
            string mainTxtPath = $"CLASSIC Data/databases/{Globals.Game} FID Main.txt";
            string dbPath = $"CLASSIC Data/databases/{Globals.Game} FormIDs.db";

            try
            {
                // Check and extract main YAML if it does not exist
                if (!File.Exists(mainYamlPath))
                {
                    using (ZipArchive zip = ZipFile.OpenRead(File.Exists(zipPath) ? zipPath : fallbackZipPath))
                    {
                        zip.ExtractToDirectory(extractPath, true);
                    }
                }

                // Check and extract main text file if it does not exist and database does not exist
                if (File.Exists(mainTxtPath) && !File.Exists(dbPath))
                {
                    DatabaseHandler.CreateFormIdDb();
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: Unable to find necessary zip archive. {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}