using System.Security.Cryptography;

namespace CLASSICCore
{
    public class XseIntegrityChecker
    {
        public static void XseCheckHashes(List<string> messageList)
        {
            Console.WriteLine("- - - INITIATED XSE FILE HASH CHECK");

            bool xseScriptMissing = false;
            bool xseScriptMismatch = false;

            Dictionary<string, string>? xseHashedScripts = YamlData.CLASSIC_Main.ReadEntry<Dictionary<string, string>>($"Game{Globals.Vr}_Info.XSE_HashedScripts");
            var gameFolderScripts = YamlData.CLASSIC_Main.ReadEntry<string>($"Game{Globals.Vr}_Info.Game_Folder_Scripts");

            var xseHashedScriptsLocal = new Dictionary<string, string>();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
            foreach (var (key, scriptPath) in from key in xseHashedScripts.Keys
                                              let scriptPath = Path.Combine(gameFolderScripts, key)
                                              where File.Exists(scriptPath)
                                              select (key, scriptPath))
            {
                using (var sha256 = SHA256.Create())
                {
                    using (var stream = File.OpenRead(scriptPath))
                    {
                        var fileHash = sha256.ComputeHash(stream);
                        xseHashedScriptsLocal[key] = BitConverter.ToString(fileHash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            foreach (var key in xseHashedScripts.Keys)
            {
                if (xseHashedScriptsLocal.ContainsKey(key))
                {
                    var hash1 = xseHashedScripts[key];
                    var hash2 = xseHashedScriptsLocal[key];
                    if (hash1 == hash2)
                    {
                        // Hashes match, do nothing
                    }
                    else if (hash2 is null)
                    {
                        messageList.Add($"❌ CAUTION : {key} Script Extender file is missing from your game Scripts folder! \n-----\n");
                        xseScriptMissing = true;
                    }
                    else
                    {
                        messageList.Add($"[!] CAUTION : {key} Script Extender file is outdated or overridden by another mod! \n-----\n");
                        xseScriptMismatch = true;
                    }
                }
            }

            if (xseScriptMissing)
            {
                var warnMissing = YamlData.CLASSIC_Fallout4.ReadEntry<string>("Warnings_XSE.Warn_Missing");
#pragma warning disable CS8604 // Possible null reference argument.
                messageList.Add(warnMissing);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            if (xseScriptMismatch)
            {
                var warnMismatch = YamlData.CLASSIC_Fallout4.ReadEntry<string>("Warnings_XSE.Warn_Mismatch");
#pragma warning disable CS8604 // Possible null reference argument.
                messageList.Add(warnMismatch);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            if (!xseScriptMissing && !xseScriptMismatch)
            {
                messageList.Add("✔️ All Script Extender files have been found and accounted for! \n-----\n");
            }

            MessageList.Report.Add(string.Join("", messageList));
        }
        public static void XseCheckIntegrity()
        {
            var failedList = new List<string>();
            var messageList = new List<string>();
            Console.WriteLine("- - - INITIATED XSE INTEGRITY CHECK");

            string? catchErrors = YamlData.CLASSIC_Main.ReadEntry<string>("catch_log_errors");
            string? xseAcronym = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.XSE_Acronym");
            string? xseLogFile = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.Docs_File_XSE");
            string? xseFullName = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.XSE_FullName");
            string? xseVerLatest = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.XSE_Ver_Latest");
            string? adlibFile = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string?>($"Game{Globals.Vr}_Info.Game_File_AddressLib");

            if (File.Exists(adlibFile) && !string.IsNullOrEmpty(adlibFile))
            {
                messageList.Add("✔️ REQUIRED: *Address Library* for Script Extender is installed! \n-----\n");
            }
            else
            {
#pragma warning disable CS8604 // Possible null reference argument.
                messageList.Add(YamlData.CLASSIC_Fallout4.ReadEntry<string>("Warnings_MODS.Warn_ADLIB_Missing"));
#pragma warning restore CS8604 // Possible null reference argument.
            }

            if (File.Exists(xseLogFile) && !string.IsNullOrEmpty(xseLogFile))
            {
                messageList.Add($"✔️ REQUIRED: *{xseFullName}* is installed! \n-----\n");
                var xseData = File.ReadAllLines(xseLogFile);

                if (xseData.Length > 0 && xseVerLatest is not null && xseData[0].Contains(xseVerLatest))
                {
                    messageList.Add($"✔️ You have the latest version of *{xseFullName}*! \n-----\n");
                }
                else
                {
                    string? warnXseOutdated = YamlData.CLASSIC_Fallout4.ReadEntry<string>("Warnings_XSE.Warn_Outdated");
                    if (warnXseOutdated is not null)
                    {
                        messageList.Add(warnXseOutdated);
                    }
                }

                foreach (var line in xseData)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if (catchErrors.Split(',').Any(item => line.Contains(item, StringComparison.OrdinalIgnoreCase)))
                    {
                        failedList.Add(line);
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }

                if (failedList.Count > 0)
                {
                    messageList.Add($"#❌ CAUTION : {xseAcronym}.log REPORTS THE FOLLOWING ERRORS #\n");
                    foreach (var elem in failedList)
                    {
                        messageList.Add($"ERROR > {elem.Trim()} \n-----\n");
                    }
                }
            }
            else
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                messageList.Add($"❌ CAUTION : *{xseAcronym.ToLower()}.log* FILE IS MISSING FROM YOUR DOCUMENTS FOLDER! \n");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                messageList.Add($"   You need to run the game at least once with {xseAcronym.ToLower()}_loader.exe \n");
                messageList.Add("    After that, try running CLASSIC again! \n-----\n");
            }
            List<string?> output = new List<string?> { messageList.ToString(), failedList.ToString() };
            MessageList.Report.Add(string.Join("", messageList));
        }
    }
}