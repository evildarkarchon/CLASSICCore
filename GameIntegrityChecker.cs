using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace CLASSICCore
{
    public class GameIntegrityChecker
    {
        public static string GameCheckIntegrity()
        {
            var messageList = new List<string>();
            Console.WriteLine("- - - INITIATED GAME INTEGRITY CHECK");

            string? steamIniLocal = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.Game_File_SteamINI");
            string? exeHashOld = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.EXE_HashedOLD");
            string? gameExeLocal = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.Game_File_EXE");
            string? rootName = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.Main_Root_Name");

#pragma warning disable CS8604 // Possible null reference argument.
            var gameExePath = new FileInfo(gameExeLocal);
            var steamIniPath = new FileInfo(steamIniLocal);
#pragma warning restore CS8604 // Possible null reference argument.
            if (gameExePath.Exists)
            {
                string exeHashLocal;
                using (var sha256 = SHA256.Create())
                {
                    using (var stream = gameExePath.OpenRead())
                    {
                        var fileContents = sha256.ComputeHash(stream);
                        exeHashLocal = BitConverter.ToString(fileContents).Replace("-", "").ToLowerInvariant();
                    }
                }

                if (exeHashLocal == exeHashOld && !steamIniPath.Exists)
                {
                    messageList.Add($"✔️ You have the latest version of {rootName}! \n-----\n");
                }
                else if (steamIniPath.Exists)
                {
                    messageList.Add($"❌ CAUTION : YOUR {rootName} GAME / EXE VERSION IS OUT OF DATE \n-----\n");
                }
                else
                {
                    messageList.Add($"❌ CAUTION : YOUR {rootName} GAME / EXE VERSION IS OUT OF DATE \n-----\n");
                }

                if (!gameExePath.FullName.Contains("Program Files"))
                {
                    messageList.Add($"✔️ Your {rootName} game files are installed outside of the Program Files folder! \n-----\n");
                }
                else
                {
                    string? rootWarn = YamlData.CLASSIC_Main.ReadEntry<string>("Warnings_GAME.warn_root_path");
#pragma warning disable CS8604 // Possible null reference argument.
                    messageList.Add(rootWarn);
#pragma warning restore CS8604 // Possible null reference argument.
                }
            }
            string messageOutput = string.Join("", messageList);
            return messageOutput;
        }
    }
}