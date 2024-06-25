namespace CLASSICCore
{
    public class FileGenerator
    {
        public static void ClassicGenerateFiles()
        {
            string ignoreFilePath = "CLASSIC Ignore.yaml";
            string localYamlPath = $"CLASSIC Data/CLASSIC {Globals.Game} Local.yaml";
            string fidModsFilePath = $"CLASSIC Data/databases/{Globals.Game} FID Mods.txt";

            try
            {
                // Generate CLASSIC Ignore.yaml if it does not exist
                if (!File.Exists(ignoreFilePath))
                {
                    string? defaultIgnoreFile = YamlData.CLASSIC_Main.ReadEntry<string>("CLASSIC_Info.default_ignorefile");
                    File.WriteAllText(ignoreFilePath, defaultIgnoreFile);
                    ClassicLogger.LogInfo($"Generated {ignoreFilePath}");
                }

                // Generate CLASSIC Local.yaml if it does not exist
                if (!File.Exists(localYamlPath))
                {
                    string? defaultLocalYaml = YamlData.CLASSIC_Main.ReadEntry<string>("CLASSIC_Info.default_localyaml");
                    File.WriteAllText(localYamlPath, defaultLocalYaml);
                    ClassicLogger.LogInfo($"Generated {localYamlPath}");
                }

                // Generate FID Mods.txt if it does not exist
                if (!File.Exists(fidModsFilePath))
                {
                    if (Globals.Game == "Fallout4")
                    {
                        string? defaultFidFile = YamlData.CLASSIC_Fallout4.ReadEntry<string>("Default_FIDMods");
                        File.WriteAllText(fidModsFilePath, defaultFidFile);
                        ClassicLogger.LogInfo($"Generated {fidModsFilePath}");
                    } /*else if (Globals.Game == "SkyrimSE") {
                        defaultFidFile = YamlData.CLASSIC_SkyrimSE.ReadOrUpdateEntry($"CLASSIC Data/databases/CLASSIC {Globals.Game}.yaml", "Default_FIDMods");
                    } else {
                        defaultFidFile = YamlData.CLASSIC_Other.ReadOrUpdateEntry($"CLASSIC Data/databases/CLASSIC {Globals.Game}.yaml", "Default_FIDMods");
                    */
                }
            }
            catch (Exception ex)
            {
                ClassicLogger.LogError($"An error occurred while generating files: {ex.Message}");
                throw;
            }
        }
    }
}