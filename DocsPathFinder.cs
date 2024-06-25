using System.Runtime.InteropServices;

namespace CLASSICCore
{
    public class DocsPathFinder
    {
        public static void DocsPathFind()
        {
            Console.WriteLine("- - - INITIATED DOCS PATH CHECK");

            string? docsName = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.Main_Docs_Name");

            static string? GetWindowsDocsPath()
            {
                string? docsName = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.Main_Docs_Name");
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string winDocs = Path.Combine(documentsPath, $"My Games\\{docsName}");
                YamlData.CLASSIC_Fallout4.UpdateEntry($"Game{Globals.Vr}_Info.Root_Folder_Docs", winDocs);
                return winDocs;
            }

            void GetLinuxDocsPath()
            {
                var gameSid = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.Main_SteamID");
                var libraryFoldersPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Steam/steamapps/common/libraryfolders.vdf");
                if (File.Exists(libraryFoldersPath))
                {
                    var lines = File.ReadAllLines(libraryFoldersPath);
                    string libraryPath = "";
                    foreach (var line in lines)
                    {
                        if (line.Contains("\"path\""))
                            libraryPath = line.Split('"')[3];
                        if (gameSid is not null && docsName is not null && line.Contains(gameSid))
                        {
                            var linuxDocs = Path.Combine(libraryPath, "steamapps/compatdata", gameSid, "pfx/drive_c/users/steamuser/My Documents/My Games", docsName);
                            YamlData.CLASSIC_Fallout4.UpdateEntry($"Game{Globals.Vr}_Info.Root_Folder_Docs", linuxDocs);
                            break;
                        }
                    }
                }
            }

            void GetManualDocsPath()
            {
                Console.WriteLine($"> > > PLEASE ENTER THE FULL DIRECTORY PATH WHERE YOUR {docsName}.ini IS LOCATED < < <");
                while (true)
                {
                    Console.Write($"(EXAMPLE: C:/Users/Zen/Documents/My Games/{docsName} | Press ENTER to confirm.)\n> ");
                    var pathInput = Console.ReadLine();
                    if (Directory.Exists(pathInput))
                    {
                        Console.WriteLine($"You entered: '{pathInput}' | This path will be automatically added to CLASSIC Settings.yaml");
                        YamlData.CLASSIC_Fallout4.UpdateEntry($"Game{Globals.Vr}_Info.Root_Folder_Docs", pathInput.Trim());
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"'{pathInput}' is not a valid or existing directory path. Please try again.");
                    }
                }
            }

            string? docsPath = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.Root_Folder_Docs");
            if (string.IsNullOrEmpty(docsPath))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    docsPath = GetWindowsDocsPath();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    GetLinuxDocsPath();
                }
                else
                {
                    GetManualDocsPath();
                }
            }

            if (!Directory.Exists(docsPath))
            {
                GetManualDocsPath();
            }
        }
    }
}