using Microsoft.Extensions.Configuration;
using System.Text;

namespace CLASSICCore
{
    public class DocsChecker
    {
        public static string? DocsCheckFolder()
        {
            var messageList = new List<string>();
            var docsName = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.Main_Docs_Name");

            if (docsName is not null && docsName.Contains("onedrive", StringComparison.CurrentCultureIgnoreCase))
            {
                var docsWarn = YamlData.CLASSIC_Main.ReadEntry<string>("Warnings_GAME.warn_docs_path");
                if (docsWarn is not null)
                {
                    messageList.Add(docsWarn);
                }
            }

            return string.Join("", messageList);
        }

        public static string? DocsCheckIni(string iniName)
        {
            var messageList = new List<string>();
            Console.WriteLine($"- - - INITIATED {iniName} CHECK");

            var folderDocs = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.Root_Folder_Docs");
            var docsName = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.Main_Docs_Name");

#pragma warning disable CS8604 // Possible null reference argument.
            var iniFileList = Directory.GetFiles(folderDocs, "*.ini").Select(Path.GetFileName).ToList();
#pragma warning restore CS8604 // Possible null reference argument.
            var iniPath = Path.Combine(folderDocs, iniName);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (iniFileList.Any(file => file.Equals(iniName, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    RemoveReadOnly(iniPath);

                    var configurationBuilder = new ConfigurationBuilder()
                        .SetBasePath(folderDocs)
                        .AddIniFile(iniName, optional: false, reloadOnChange: false);
                    IConfigurationRoot configuration = configurationBuilder.Build();

                    messageList.Add($"✔️ No obvious corruption detected in {iniName}, file seems OK! \n-----\n");

                    if (iniName.Equals($"{docsName}Custom.ini", StringComparison.OrdinalIgnoreCase))
                    {
                        var section = configuration.GetSection("Archive");
                        if (!section.Exists())
                        {
                            messageList.AddRange(new[]
                            {
                            "❌ WARNING : Archive Invalidation / Loose Files setting is not enabled. \n",
                            "  CLASSIC will now enable this setting automatically in the game INI files. \n-----\n"
                        });

                            // Adding the section and keys
                            var iniData = new Dictionary<string, string>
                        {
                            { "Archive:bInvalidateOlderFiles", "1" },
                            { "Archive:sResourceDataDirsFinal", "" }
                        };
                            SaveIniFile(iniPath, iniData);
                        }
                        else
                        {
                            messageList.Add("✔️ Archive Invalidation / Loose Files setting is already enabled! \n-----\n");
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    messageList.AddRange(new[]
                    {
                    $"[!] CAUTION : YOUR {iniName} FILE IS SET TO READ ONLY. \n",
                    "     PLEASE REMOVE THE READ ONLY PROPERTY FROM THIS FILE, \n",
                    "     SO CLASSIC CAN MAKE THE REQUIRED CHANGES TO IT. \n-----\n"
                });
                }
                catch (Exception)
                {
                    messageList.AddRange(new[]
                    {
                    $"[!] CAUTION : YOUR {iniName} FILE IS VERY LIKELY BROKEN, PLEASE CREATE A NEW ONE \n",
                    $"    Delete this file from your Documents/My Games/{docsName} folder, then press \n",
                    $"    *Scan Game Files* in CLASSIC to generate a new {iniName} file. \n-----\n"
                });
                }
            }
            else
            {
                if (iniName.Equals($"{docsName}.ini", StringComparison.OrdinalIgnoreCase))
                {
                    messageList.AddRange(new[]
                    {
                    $"❌ CAUTION : {iniName} FILE IS MISSING FROM YOUR DOCUMENTS FOLDER! \n",
                    $"   You need to run the game at least once with {docsName}Launcher.exe \n",
                    "    This will create files and INI settings required for the game to run. \n-----\n"
                });
                }

                if (iniName.Equals($"{docsName}Custom.ini", StringComparison.OrdinalIgnoreCase))
                {
                    using (var iniFile = new StreamWriter(iniPath, false, Encoding.UTF8))
                    {
                        messageList.AddRange(new[]
                        {
                        "❌ WARNING : Archive Invalidation / Loose Files setting is not enabled. \n",
                        "  CLASSIC will now enable this setting automatically in the game INI files. \n-----\n"
                    });

                        var customIniConfig = YamlData.CLASSIC_Main.ReadEntry<string>("Default_CustomINI");
                        iniFile.Write(customIniConfig);
                    }
                }
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            return string.Join("", messageList);
        }
        private static void RemoveReadOnly(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.IsReadOnly)
            {
                fileInfo.IsReadOnly = false;
            }
        }

        private static void SaveIniFile(string iniPath, IDictionary<string, string> data)
        {
            var iniContent = new StringBuilder();
            foreach (var kvp in data)
            {
                var sectionKey = kvp.Key.Split(':');
                if (sectionKey.Length == 2)
                {
                    iniContent.AppendLine($"[{sectionKey[0]}]");
                    iniContent.AppendLine($"{sectionKey[1]}={kvp.Value}");
                }
            }
            File.WriteAllText(iniPath, iniContent.ToString(), Encoding.UTF8);
        }
    }
}