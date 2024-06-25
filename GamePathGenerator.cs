namespace CLASSICCore
{
    public class GamePathGenerator
    {
        public static void GameGeneratePaths()
        {
            Console.WriteLine("- - - INITIATED GAME PATH GENERATION");

            string? gamePath = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.Root_Folder_Game");
            string? xseAcronym = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.XSE_Acronym");
            string? xseAcronymBase = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game_Info.XSE_Acronym");

#pragma warning disable CS8604 // Possible null reference argument.
            YamlData.CLASSIC_Fallout4_Local.UpdateEntry($"Game{Globals.Vr}_Info.Game_Folder_Data", Path.Combine(gamePath, "Data"));

            YamlData.CLASSIC_Fallout4_Local.UpdateEntry($"Game{Globals.Vr}_Info.Game_Folder_Scripts", Path.Combine(gamePath, "Data", "Scripts"));
            YamlData.CLASSIC_Fallout4_Local.UpdateEntry($"Game{Globals.Vr}_Info.Game_Folder_Plugins", Path.Combine(gamePath, "Data", xseAcronymBase, "Plugins"));
            YamlData.CLASSIC_Fallout4_Local.UpdateEntry($"Game{Globals.Vr}_Info.Game_File_SteamINI", Path.Combine(gamePath, "steam_api.ini"));
            YamlData.CLASSIC_Fallout4_Local.UpdateEntry($"Game{Globals.Vr}_Info.Game_File_EXE", Path.Combine(gamePath, $"{Globals.Game}{Globals.Vr}.exe"));
#pragma warning restore CS8604 // Possible null reference argument.

            if (Globals.Game == "Fallout4")
            {
                if (string.IsNullOrEmpty(Globals.Vr))
                {
                    YamlData.CLASSIC_Fallout4_Local.UpdateEntry("Game_Info.Game_File_AddressLib", Path.Combine(gamePath, "Data", xseAcronymBase, "plugins", "version-1-10-163-0.bin"));
                }
                else
                {
                    YamlData.CLASSIC_Fallout4_Local.UpdateEntry("GameVR_Info.Game_File_AddressLib", Path.Combine(gamePath, "Data", xseAcronymBase, "plugins", "version-1-2-72-0.csv"));
                }
            }
        }
    }
}