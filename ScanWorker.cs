using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace CLASSICCore
{
    public class ScanWorker
    {
        public static void CrashlogsReformat()
        {
            ClassicLogger.LogDebug("- - - INITIATED CRASH LOG FILE REFORMAT");

            var xseAcronym = YamlData.CLASSIC_Fallout4.ReadEntry($"Game{Globals.Vr}_Info.XSE_Acronym");
            List<string>? removeList = YamlData.CLASSIC_Main.ReadEntry<List<string>>("exclude_log_records");
            bool? simpleLogs = YamlData.Settings_Query<bool>("Simplify Logs");

            List<string> crashFiles = CrashlogsGetFiles();

            foreach (var file in crashFiles)
            {
                var crashData = File.ReadAllLines(file, Encoding.UTF8).ToList();
                int indexPlugins;
                try
                {
                    indexPlugins = crashData.FindIndex(item => xseAcronym != null && !item.Contains(xseAcronym) && item.Contains("PLUGINS:"));
                    if (indexPlugins == -1) throw new Exception();
                }
                catch
                {
                    indexPlugins = 1;
                }

                for (int index = 0; index < crashData.Count; index++)
                {
                    string line = crashData[index];
                    if (simpleLogs == true)
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        if (removeList.Any(removeString => line.Contains(removeString)))
                        {
                            crashData.RemoveAt(index); // Remove *useless* lines from crash log if Simplify Logs is enabled.
                            index--; // Adjust index since we removed the current line.
                        }
                        else if (index > indexPlugins)
                        {
                            string formattedLine = Regex.Replace(line, @"\[(.*?)]", m => "[" + Regex.Replace(m.Groups[1].Value, @"\s", "0") + "]");
                            crashData[index] = formattedLine;
                        }
#pragma warning restore CS8604 // Possible null reference argument.

                    }
                }

                File.WriteAllLines(file, crashData, Encoding.UTF8);
            }
        }
        private readonly Stopwatch _timer = new Stopwatch();
        public void ClassicFormatLogs()
        {
            Console.WriteLine("Worker.DoWork");
        }
        public static List<string> CrashlogsGetFiles()
        {
            ClassicLogger.LogDebug("- - - INITIATED CRASH LOG FILE LIST GENERATION");

            var CLASSIC_folder = Directory.GetCurrentDirectory();
            var CUSTOM_folder = YamlData.Settings_Query<string>("SCAN Custom Path");
            var XSE_folder = YamlData.CLASSIC_Fallout4_Local.ReadEntry($"Game{Globals.Vr}_Info.Docs_Folder_XSE");

            List<string> crashFiles = new List<string>();

            if (Directory.Exists(XSE_folder))
            {
                var xseCrashFiles = Directory.GetFiles(XSE_folder, "crash-*.log");
                if (xseCrashFiles.Length > 0)
                {
                    foreach (var crashFile in xseCrashFiles)
                    {
                        string destinationFile = Path.Combine(CLASSIC_folder, Path.GetFileName(crashFile));
                        if (!File.Exists(destinationFile))
                        {
                            File.Copy(crashFile, destinationFile);
                        }
                    }
                }
            }

            crashFiles.AddRange(Directory.GetFiles(CLASSIC_folder, "crash-*.log"));

            if (!string.IsNullOrEmpty(CUSTOM_folder))
            {
                if (Directory.Exists(CUSTOM_folder))
                {
                    crashFiles.AddRange(Directory.GetFiles(CUSTOM_folder, "crash-*.log"));
                }
            }

            return crashFiles;
        }
        private static readonly Random random = new Random();

        public static void CrashlogsScan()
        {
            Console.WriteLine("REFORMATTING CRASH LOGS, PLEASE WAIT...\n");
            CrashlogsReformat();

            Console.WriteLine("SCANNING CRASH LOGS, PLEASE WAIT...\n");
            var scanStartTime = Stopwatch.StartNew();

            var crashLogList = CrashlogsGetFiles();
            var scanFailedList = new List<string>();
            var scanFolder = Directory.GetCurrentDirectory();
            var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var scanInvalidList = Directory.GetFiles(scanFolder, "crash-*.txt").ToList();
            int statsCrashlogScanned = 0, statsCrashlogIncomplete = 0, statsCrashlogFailed = 0;

            foreach (var crashlogFile in crashLogList)
            {
                var autoscanReport = new List<string>();
                bool triggerPluginLimit = false, triggerPluginsLoaded = false, triggerScanFailed = false;

                string[] crashData = File.ReadAllLines(crashlogFile, Encoding.UTF8);

                autoscanReport.AddRange(new[]
                {
                    $"{Path.GetFileName(crashlogFile)} -> AUTOSCAN REPORT GENERATED BY {ClassicScanLogsVariables.ClassicVersion} \n",
                    "# FOR BEST VIEWING EXPERIENCE OPEN THIS FILE IN NOTEPAD++ OR SIMILAR # \n",
                    "# PLEASE READ EVERYTHING CAREFULLY AND BEWARE OF FALSE POSITIVES # \n",
                    "====================================================\n"
                });

                // 1) CHECK EXISTENCE AND INDEXES OF EACH SEGMENT
                int indexCrashgenver = crashData.Take(10)
                            .Select((item, index) => new { Item = item, Index = index })
                            .FirstOrDefault(x => ClassicScanLogsVariables.CrashGenName != null &&
                                                 x.Item.ToLower().Contains(ClassicScanLogsVariables.CrashGenName.ToLower()))
                            ?.Index ?? -1;
                if (indexCrashgenver == -1) indexCrashgenver = 1;

                int indexMainerror = crashData.Take(10)
                    .Select((item, index) => new { Item = item, Index = index })
                    .FirstOrDefault(x => x.Item.ToLower().Contains("unhandled exception"))
                    ?.Index ?? -1;
                if (indexMainerror == -1) indexMainerror = 3;


                // Helper method to generate segments
                List<string> CrashlogGenerateSegment(string segmentStart, string segmentEnd)
                {
                    int indexStart = Array.FindIndex(crashData, item => item.ToLower().Contains(segmentStart.ToLower())) + 1;
                    int indexEnd = Array.FindIndex(crashData, item => item.ToLower().Contains(segmentEnd.ToLower()) && !item.ToLower().Contains(ClassicScanLogsVariables.XseAcronym?.ToLower() ?? "")) - 1;

                    if (indexStart <= indexEnd)
                    {
                        return crashData[indexStart..indexEnd]
                            .Where(s_line => !ClassicScanLogsVariables.RemoveList!.Any(item => s_line.ToLower().Contains(item.ToLower())))
                            .Select(s_line => s_line.Trim())
                            .ToList();
                    }
                    return new List<string>();
                }

                // 2) GENERATE REQUIRED SEGMENTS FROM THE CRASH LOG
                var segmentAllmodules = CrashlogGenerateSegment("modules:", $"{ClassicScanLogsVariables.XseAcronym?.ToLower()} plugins:");
                var segmentXsemodules = CrashlogGenerateSegment($"{ClassicScanLogsVariables.XseAcronym?.ToLower()} plugins:", "plugins:");
                var segmentCallstack = CrashlogGenerateSegment("probable call stack:", "modules:");
                var segmentCrashgen = CrashlogGenerateSegment("[compatibility]", "system specs:");
                var segmentSystem = CrashlogGenerateSegment("system specs:", "probable call stack:");
                var segmentPlugins = CrashlogGenerateSegment("plugins:", "???????");
                string segmentCallstackIntact = string.Join("", segmentCallstack);

                if (!segmentPlugins.Any())
                {
                    statsCrashlogIncomplete++;
                }

                if (crashData.Length < 20)
                {
                    statsCrashlogScanned--;
                    statsCrashlogFailed++;
                    triggerScanFailed = true;
                }

                // MAIN ERROR
                string crashlogMainerror;
                try
                {
                    crashlogMainerror = crashData[indexMainerror];
                    if (crashlogMainerror.Contains('|'))
                    {
                        var crashlogErrorsplit = crashlogMainerror.Split('|', 2);
                        autoscanReport.Add($"\nMain Error: {crashlogErrorsplit[0]}\n{crashlogErrorsplit[1]}\n");
                    }
                    else
                    {
                        autoscanReport.Add($"\nMain Error: {crashlogMainerror}\n");
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    crashlogMainerror = "UNKNOWN";
                    autoscanReport.Add($"\nMain Error: {crashlogMainerror}\n");
                }

                // CRASHGEN VERSION
                string crashlogCrashgen = crashData[indexCrashgenver].Trim();
                autoscanReport.Add($"Detected {ClassicScanLogsVariables.CrashGenName} Version: {crashlogCrashgen} \n");
                if (ClassicScanLogsVariables.CrashGenLatestOg == crashlogCrashgen || ClassicScanLogsVariables.CrashGenLatestVr == crashlogCrashgen)
                {
                    autoscanReport.Add($"* You have the latest version of {ClassicScanLogsVariables.CrashGenName}! *\n\n");
                }
                else
                {
                    autoscanReport.Add($"{ClassicScanLogsVariables.WarnOutdated} \n");
                }

                // REQUIRED LISTS, DICTS AND CHECKS
                List<string> ignorePluginsList = new List<string>();
                if (!string.IsNullOrEmpty(ClassicScanLogsVariables.IgnoreList))
                {
                    ignorePluginsList = ClassicScanLogsVariables.IgnoreList
                        .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(item => item.Trim().ToLower())
                        .ToList();
                }

                bool crashlogGPUAMD = false, crashlogGPUNV = false, crashlogGPUI;
                var crashlogPlugins = new Dictionary<string, string>();

                if (Globals.Game == "Fallout4")
                {
                    if (segmentPlugins.Any(elem => elem.Contains("Fallout4.esm")))
                    {
                        triggerPluginsLoaded = true;
                    }
                    else
                    {
                        statsCrashlogIncomplete++;
                    }
                }
                else if (Globals.Game == "SkyrimSE")
                {
                    if (segmentPlugins.Any(elem => elem.Contains("Skyrim.esm")))
                    {
                        triggerPluginsLoaded = true;
                    }
                    else
                    {
                        statsCrashlogIncomplete++;
                    }
                }

                // CHECK GPU TYPE FOR CRASH LOG
                crashlogGPUAMD = segmentSystem.Any(elem => elem.Contains("GPU #1") && elem.Contains("AMD"));
                crashlogGPUNV = segmentSystem.Any(elem => elem.Contains("GPU #1") && elem.Contains("Nvidia"));
                crashlogGPUI = !crashlogGPUAMD && !crashlogGPUNV;

                // IF LOADORDER FILE EXISTS, USE ITS PLUGINS
                if (File.Exists("loadorder.txt"))
                {
                    autoscanReport.AddRange(new[]
                    {
                        "* ✔️ LOADORDER.TXT FILE FOUND IN THE MAIN CLASSIC FOLDER! *\n",
                        "CLASSIC will now ignore plugins in all crash logs and only detect plugins in this file.\n",
                        "[ To disable this functionality, simply remove loadorder.txt from your CLASSIC folder. ]\n\n"
                    });

                    var loadorderData = File.ReadAllLines("loadorder.txt", Encoding.UTF8);
                    foreach (var elem in loadorderData.Skip(1))
                    {
                        if (!crashlogPlugins.ContainsKey(elem))
                        {
                            crashlogPlugins[elem] = "LO";
                        }
                    }
                    triggerPluginsLoaded = true;
                }
                else // OTHERWISE, USE PLUGINS FROM CRASH LOG
                {
                    foreach (var elem in segmentPlugins)
                    {
                        if (elem.Contains("[FF]"))
                        {
                            triggerPluginLimit = true;
                        }
                        if (elem.Contains(" "))
                        {
                            var elemParts = elem.Replace("     ", " ").Trim().Split(' ', 2);
                            elemParts[0] = elemParts[0].Replace("[", "").Replace(":", "").Replace("]", "");
                            crashlogPlugins[elemParts[1]] = elemParts[0];
                        }
                    }
                }

                foreach (var elem in segmentXsemodules)
                {
                    // SOME IMPORTANT DLLs HAVE A VERSION, REMOVE IT
                    var elemTrimmed = elem.Trim();
                    if (elemTrimmed.Contains(".dll v"))
                    {
                        elemTrimmed = elemTrimmed.Split(" v", 2)[0];
                    }
                    if (!crashlogPlugins.ContainsKey(elemTrimmed))
                    {
                        crashlogPlugins[elemTrimmed] = "DLL";
                    }
                }

                foreach (var elem in segmentAllmodules)
                {
                    // SOME IMPORTANT DLLs ONLY APPEAR UNDER ALL MODULES
                    if (elem.ToLower().Contains("vulkan"))
                    {
                        var elemParts = elem.Trim().Split(' ', 2);
                        if (!crashlogPlugins.ContainsKey(elemParts[0]))
                        {
                            crashlogPlugins[elemParts[0]] = "DLL";
                        }
                    }
                }

                // CHECK IF THERE ARE ANY PLUGINS IN THE IGNORE TOML
                if (ignorePluginsList != null)
                {
                    foreach (var item in ignorePluginsList)
                    {
                        crashlogPlugins.Remove(item);
                    }
                }

                autoscanReport.AddRange(new[]
                {
                    "====================================================\n",
                    "CHECKING IF LOG MATCHES ANY KNOWN CRASH SUSPECTS...\n",
                    "====================================================\n"
                });

                if (crashlogMainerror.ToLower().Contains(".dll") && !crashlogMainerror.ToLower().Contains("tbbmalloc"))
                {
                    autoscanReport.AddRange(new[]
                    {
                        "* NOTICE : MAIN ERROR REPORTS THAT A DLL FILE WAS INVOLVED IN THIS CRASH! * \n",
                        "If that dll file belongs to a mod, that mod is a prime suspect for the crash. \n-----\n"
                    });
                }

                const int maxWarnLength = 30;
                bool triggerSuspectFound = false;

                foreach (var error in ClassicScanLogsVariables.SuspectsErrorList!.Split('\n'))
                {
                    var errorSplit = error.Split(" | ", 2);
                    if (crashlogMainerror.Contains(errorSplit[1]))
                    {
                        errorSplit[1] = errorSplit[1].PadRight(maxWarnLength, '.');
                        autoscanReport.Add($"# Checking for {errorSplit[1]} SUSPECT FOUND! > Severity : {errorSplit[0]} # \n-----\n");
                        triggerSuspectFound = true;
                    }
                }

                foreach (var key in ClassicScanLogsVariables.SuspectsStackList!.Split('\n'))
                {
                    var keySplit = key.Split(" | ", 2);
                    bool errorReqFound = false, errorOptFound = false, stackFound = false;
                    var itemList = keySplit[1].Split(',');
                    bool hasRequiredItem = itemList.Any(elem => elem.StartsWith("ME-REQ|"));

                    foreach (var item in itemList)
                    {
                        if (item.Contains("|"))
                        {
                            var itemSplit = item.Split('|', 2);
                            if (itemSplit[0] == "ME-REQ")
                            {
                                if (crashlogMainerror.Contains(itemSplit[1]))
                                {
                                    errorReqFound = true;
                                }
                            }
                            else if (itemSplit[0] == "ME-OPT")
                            {
                                if (crashlogMainerror.Contains(itemSplit[1]))
                                {
                                    errorOptFound = true;
                                }
                            }
                            else if (int.TryParse(itemSplit[0], out int count))
                            {
                                if (Regex.Matches(segmentCallstackIntact, itemSplit[1]).Count >= count)
                                {
                                    stackFound = true;
                                }
                            }
                            else if (itemSplit[0] == "NOT")
                            {
                                if (segmentCallstackIntact.Contains(itemSplit[1]))
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (segmentCallstackIntact.Contains(item))
                            {
                                stackFound = true;
                            }
                        }
                    }

                    if (hasRequiredItem)
                    {
                        if (errorReqFound)
                        {
                            keySplit[1] = keySplit[1].PadRight(maxWarnLength, '.');
                            autoscanReport.Add($"# Checking for {keySplit[1]} SUSPECT FOUND! > Severity : {keySplit[0]} # \n-----\n");
                            triggerSuspectFound = true;
                        }
                    }
                    else
                    {
                        if (errorOptFound || stackFound)
                        {
                            keySplit[1] = keySplit[1].PadRight(maxWarnLength, '.');
                            autoscanReport.Add($"# Checking for {keySplit[1]} SUSPECT FOUND! > Severity : {keySplit[0]} # \n-----\n");
                            triggerSuspectFound = true;
                        }
                    }
                }
                if (triggerSuspectFound)
                {
                    autoscanReport.AddRange(new[]
                    {
                        "* FOR DETAILED DESCRIPTIONS AND POSSIBLE SOLUTIONS TO ANY ABOVE DETECTED CRASH SUSPECTS *\n",
                        "* SEE: https://docs.google.com/document/d/17FzeIMJ256xE85XdjoPvv_Zi3C5uHeSTQh6wOZugs4c *\n\n"
                    });
                }
                else
                {
                    autoscanReport.AddRange(new[]
                    {
                        "# FOUND NO CRASH ERRORS / SUSPECTS THAT MATCH THE CURRENT DATABASE #\n",
                        "Check below for mods that can cause frequent crashes and other problems.\n\n"
                    });
                }

                autoscanReport.AddRange(new[]
                {
                    "====================================================\n",
                    "CHECKING IF NECESSARY FILES/SETTINGS ARE CORRECT...\n",
                    "====================================================\n"
                });

                if (!YamlData.Settings_Query<bool>("FCX Mode"))
                {
                    autoscanReport.AddRange(new[]
                    {
                        "* NOTICE: FCX MODE IS DISABLED. YOU CAN ENABLE IT TO DETECT PROBLEMS IN YOUR MOD & GAME FILES * \n",
                        "[ FCX Mode can be enabled in the exe or CLASSIC Settings.yaml located in your CLASSIC folder. ] \n\n"
                    });

                    foreach (var line in segmentCrashgen)
                    {
                        if (line.ToLower().Contains("false") && !ClassicScanLogsVariables.CrashGenIgnore!.Split(',').Any(elem => line.ToLower().Contains(elem.ToLower())))
                        {
                            var lineSplit = line.Split(':', 2);
                            autoscanReport.Add($"* NOTICE : {lineSplit[0].Trim()} is disabled in your {ClassicScanLogsVariables.CrashGenName} settings, is this intentional? * \n-----\n");
                        }

                        if (line.ToLower().Contains("achievements:"))
                        {
                            if (line.ToLower().Contains("true") && segmentXsemodules.Any(elem => elem.ToLower().Contains("achievements.dll") || elem.ToLower().Contains("unlimitedsurvivalmode.dll")))
                            {
                                autoscanReport.AddRange(new[]
                                {
                                    "# ❌ CAUTION : The Achievements Mod and/or Unlimited Survival Mode is installed, but Achievements is set to TRUE # \n",
                                    $" FIX: Open {ClassicScanLogsVariables.CrashGenName}'s TOML file and change Achievements to FALSE, this prevents conflicts with {ClassicScanLogsVariables.CrashGenName}.\n-----\n"
                                });
                            }
                            else
                            {
                                autoscanReport.Add($"✔️ Achievements parameter is correctly configured in your {ClassicScanLogsVariables.CrashGenName} settings! \n-----\n");
                            }
                        }

                        if (line.ToLower().Contains("memorymanager:"))
                        {
                            if (line.ToLower().Contains("true") && segmentXsemodules.Any(elem => elem.ToLower().Contains("bakascrapheap.dll")))
                            {
                                autoscanReport.AddRange(new[]
                                {
                                    "# ❌ CAUTION : The Baka ScrapHeap Mod is installed, but MemoryManager parameter is set to TRUE # \n",
                                    $" FIX: Open {ClassicScanLogsVariables.CrashGenName}'s TOML file and change MemoryManager to FALSE, this prevents conflicts with {ClassicScanLogsVariables.CrashGenName}.\n-----\n"
                                });
                            }
                            else
                            {
                                autoscanReport.Add($"✔️ Memory Manager parameter is correctly configured in your {ClassicScanLogsVariables.CrashGenName} settings! \n-----\n");
                            }
                        }

                        if (line.ToLower().Contains("f4ee:"))
                        {
                            if (line.ToLower().Contains("false") && segmentXsemodules.Any(elem => elem.ToLower().Contains("f4ee.dll")))
                            {
                                autoscanReport.AddRange(new[]
                                {
                                    "# ❌ CAUTION : Looks Menu is installed, but F4EE parameter under [Compatibility] is set to FALSE # \n",
                                    $" FIX: Open {ClassicScanLogsVariables.CrashGenName}'s TOML file and change F4EE to TRUE, this prevents bugs and crashes from Looks Menu.\n-----\n"
                                });
                            }
                            else
                            {
                                autoscanReport.Add($"✔️ F4EE (Looks Menu) parameter is correctly configured in your {ClassicScanLogsVariables.CrashGenName} settings! \n-----\n");
                            }
                        }
                    }
                }
                else
                {
                    autoscanReport.AddRange(new[]
                    {
                        "* NOTICE: FCX MODE IS ENABLED. CLASSIC MUST BE RUN BY THE ORIGINAL USER FOR CORRECT DETECTION * \n",
                        "[ To disable mod & game files detection, disable FCX Mode in the exe or CLASSIC Settings.yaml ] \n\n"
                    });
                }

                // TODO: Implement main_files_check and game_files_check

                autoscanReport.AddRange(new[]
                {
                    "====================================================\n",
                    "CHECKING FOR MODS THAT CAN CAUSE FREQUENT CRASHES...\n",
                    "====================================================\n"
                });

                if (triggerPluginsLoaded)
                {
                    if (DetectModsSingle(ClassicScanLogsVariables.GameModsFreq!, crashlogPlugins, autoscanReport))
                    {
                        autoscanReport.AddRange(new[]
                        {
                            "# [!] CAUTION : ANY ABOVE DETECTED MODS HAVE A MUCH HIGHER CHANCE TO CRASH YOUR GAME! #\n",
                            "* YOU CAN DISABLE ANY / ALL OF THEM TEMPORARILY TO CONFIRM THEY CAUSED THIS CRASH. * \n\n"
                        });
                    }
                    else
                    {
                        autoscanReport.AddRange(new[]
                        {
                            "# FOUND NO PROBLEMATIC MODS THAT MATCH THE CURRENT DATABASE FOR THIS CRASH LOG #\n",
                            "THAT DOESN'T MEAN THERE AREN'T ANY! YOU SHOULD RUN PLUGIN CHECKER IN WRYE BASH \n",
                            "Plugin Checker Instructions: https://www.nexusmods.com/fallout4/articles/4141 \n\n"
                        });
                    }
                }
                else
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    autoscanReport.Add(ClassicScanLogsVariables.WarnNoPlugins);
#pragma warning restore CS8604 // Possible null reference argument.
                }

                autoscanReport.AddRange(new[]
                {
                    "====================================================\n",
                    "CHECKING FOR MODS THAT CONFLICT WITH OTHER MODS...\n",
                    "====================================================\n"
                });

                if (triggerPluginsLoaded)
                {
                    if (DetectModsDouble(ClassicScanLogsVariables.GameModsConf!, crashlogPlugins, autoscanReport))
                    {
                        autoscanReport.AddRange(new[]
                        {
                            "# [!] CAUTION : FOUND MODS THAT ARE INCOMPATIBLE OR CONFLICT WITH YOUR OTHER MODS # \n",
                            "* YOU SHOULD CHOOSE WHICH MOD TO KEEP AND DISABLE OR COMPLETELY REMOVE THE OTHER MOD * \n\n"
                        });
                    }
                    else
                    {
                        autoscanReport.Add("# FOUND NO MODS THAT ARE INCOMPATIBLE OR CONFLICT WITH YOUR OTHER MODS # \n\n");
                    }
                }
                else
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    autoscanReport.Add(ClassicScanLogsVariables.WarnNoPlugins);
#pragma warning restore CS8604 // Possible null reference argument.
                }

                autoscanReport.AddRange(new[]
                {
                    "====================================================\n",
                    "CHECKING FOR MODS WITH SOLUTIONS & COMMUNITY PATCHES\n",
                    "====================================================\n"
                });

                if (triggerPluginsLoaded)
                {
                    if (DetectModsSingle(ClassicScanLogsVariables.GameModsSolu!, crashlogPlugins, autoscanReport))
                    {
                        autoscanReport.AddRange(new[]
                        {
                            "# [!] CAUTION : FOUND PROBLEMATIC MODS WITH SOLUTIONS AND COMMUNITY PATCHES # \n",
                            "[Due to limitations, CLASSIC will show warnings for some mods even if fixes or patches are already installed.] \n",
                            "[To hide these warnings, you can add their plugin names to the CLASSIC Ignore.yaml file. ONE PLUGIN PER LINE.] \n\n"
                        });
                    }
                    else
                    {
                        autoscanReport.Add("# FOUND NO PROBLEMATIC MODS WITH AVAILABLE SOLUTIONS AND COMMUNITY PATCHES # \n\n");
                    }
                }
                else
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    autoscanReport.Add(ClassicScanLogsVariables.WarnNoPlugins);
#pragma warning restore CS8604 // Possible null reference argument.
                }

                if (Globals.Game == "Fallout4")
                {
                    autoscanReport.AddRange(new[]
                    {
                        "====================================================\n",
                        "CHECKING FOR MODS PATCHED THROUGH OPC INSTALLER...\n",
                        "====================================================\n"
                    });

                    if (triggerPluginsLoaded)
                    {
                        if (DetectModsSingle(ClassicScanLogsVariables.GameModsOpc2!, crashlogPlugins, autoscanReport))
                        {
                            autoscanReport.AddRange(new[]
                            {
                                "\n* FOR PATCH REPOSITORY THAT PREVENTS CRASHES AND FIXES PROBLEMS IN THESE AND OTHER MODS,* \n",
                                "* VISIT OPTIMIZATION PATCHES COLLECTION: https://www.nexusmods.com/fallout4/mods/54872 * \n\n"
                            });
                        }
                        else
                        {
                            autoscanReport.Add("# FOUND NO PROBLEMATIC MODS THAT ARE ALREADY PATCHED THROUGH THE OPC INSTALLER # \n\n");
                        }
                    }
                    else
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        autoscanReport.Add(ClassicScanLogsVariables.WarnNoPlugins);
#pragma warning restore CS8604 // Possible null reference argument.
                    }
                }

                autoscanReport.AddRange(new[]
                {
                    "====================================================\n",
                    "CHECKING IF IMPORTANT PATCHES & FIXES ARE INSTALLED\n",
                    "====================================================\n"
                });

                if (triggerPluginsLoaded)
                {
                    DetectModsImportant(ClassicScanLogsVariables.GameModsCore!, crashlogPlugins, autoscanReport, crashlogGPUAMD, crashlogGPUNV, crashlogGPUI);
                }
                else
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    autoscanReport.Add(ClassicScanLogsVariables.WarnNoPlugins);
#pragma warning restore CS8604 // Possible null reference argument.
                }

                autoscanReport.AddRange(new[]
                {
                    "====================================================\n",
                    "SCANNING THE LOG FOR SPECIFIC (POSSIBLE) SUSPECTS...\n",
                    "====================================================\n"
                });

                if (triggerPluginLimit)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    autoscanReport.Add(YamlData.CLASSIC_Main.ReadEntry("Mods_Warn.Mods_Plugin_Limit"));
#pragma warning restore CS8604 // Possible null reference argument.
                }

                autoscanReport.Add("# LIST OF (POSSIBLE) PLUGIN SUSPECTS #\n");
                var pluginsMatches = new List<string>();
                foreach (var line in segmentCallstack)
                {
                    foreach (var plugin in crashlogPlugins.Keys)
                    {
                        if (line.ToLower().Contains(plugin.ToLower()) && !line.ToLower().Contains("modified by:"))
                        {
                            if (!ClassicScanLogsVariables.GameIgnorePlugins!.Split(',').Any(ignore => plugin.ToLower().Contains(ignore.ToLower())))
                            {
                                pluginsMatches.Add(plugin);
                            }
                        }
                    }
                }

                if (pluginsMatches.Any())
                {
                    var pluginsFound = pluginsMatches.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
                    foreach (var kvp in pluginsFound)
                    {
                        autoscanReport.Add($"- {kvp.Key} | {kvp.Value}\n");
                    }

                    autoscanReport.AddRange(new[]
                    {
                        "\n[Last number counts how many times each Plugin Suspect shows up in the crash log.]\n",
                        $"These Plugins were caught by {ClassicScanLogsVariables.CrashGenName} and some of them might be responsible for this crash.\n",
                        "You can try disabling these plugins and check if the game still crashes, though this method can be unreliable.\n\n"
                    });
                }
                else
                {
                    autoscanReport.Add("* COULDN'T FIND ANY PLUGIN SUSPECTS *\n\n");
                }

                autoscanReport.Add("# LIST OF (POSSIBLE) FORM ID SUSPECTS #\n");
                var formidsMatches = segmentCallstack
                    .Where(line => line.ToLower().Contains("id:") && !line.Contains("0xFF"))
                    .Select(line => line.Replace("0x", "").Trim())
                    .ToList();

                if (formidsMatches.Any())
                {
                    var formidsFound = formidsMatches.GroupBy(x => x).OrderBy(g => g.Key).ToDictionary(g => g.Key, g => g.Count());
                    foreach (var kvp in formidsFound)
                    {
                        var formidSplit = kvp.Key.Split(": ", 2);
                        foreach (var plugin in crashlogPlugins)
                        {
                            if (formidSplit.Length >= 2 && plugin.Value == formidSplit[1].Substring(0, 2))
                            {
                                if (YamlData.Settings_Query<bool>("Show FormID Values"))
                                {
                                    // TODO: Implement FormID database lookup
                                    autoscanReport.Add($"- {kvp.Key} | [{plugin.Key}] | {kvp.Value}\n");
                                }
                                else
                                {
                                    autoscanReport.Add($"- {kvp.Key} | [{plugin.Key}] | {kvp.Value}\n");
                                }
                                break;
                            }
                        }
                    }
                    autoscanReport.AddRange(new[]
                    {
                        "\n[Last number counts how many times each Form ID shows up in the crash log.]\n",
                        $"These Form IDs were caught by {ClassicScanLogsVariables.CrashGenName} and some of them might be related to this crash.\n",
                        "You can try searching any listed Form IDs in xEdit and see if they lead to relevant records.\n\n"
                    });
                }
                else
                {
                    autoscanReport.Add("* COULDN'T FIND ANY FORM ID SUSPECTS *\n\n");
                }

                autoscanReport.Add("# LIST OF DETECTED (NAMED) RECORDS #\n");
                var recordsMatches = new List<string>();
                foreach (var line in segmentCallstack)
                {
                    if (ClassicScanLogsVariables.CatchRecords!.Split(',').Any(item => line.ToLower().Contains(item.ToLower())))
                    {
                        if (!ClassicScanLogsVariables.GameIgnoreRecords!.Split(',').Any(record => line.ToLower().Contains(record.ToLower())))
                        {
                            if (line.Contains("[RSP+"))
                            {
                                recordsMatches.Add(line.Substring(30).Trim());
                            }
                            else
                            {
                                recordsMatches.Add(line.Trim());
                            }
                        }
                    }
                }

                if (recordsMatches.Any())
                {
                    var recordsFound = recordsMatches.GroupBy(x => x).OrderBy(g => g.Key).ToDictionary(g => g.Key, g => g.Count());
                    foreach (var kvp in recordsFound)
                    {
                        autoscanReport.Add($"- {kvp.Key} | {kvp.Value}\n");
                    }

                    autoscanReport.AddRange(new[]
                    {
                        "\n[Last number counts how many times each Named Record shows up in the crash log.]\n",
                        $"These records were caught by {ClassicScanLogsVariables.CrashGenName} and some of them might be related to this crash.\n",
                        "Named records should give extra info on involved game objects, record types or mod files.\n\n"
                    });
                }
                else
                {
                    autoscanReport.Add("* COULDN'T FIND ANY NAMED RECORDS *\n\n");
                }

                if (Globals.Game == "Fallout4")
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    autoscanReport.Add(ClassicScanLogsVariables.AutoscanText);
#pragma warning restore CS8604 // Possible null reference argument.
                }
                autoscanReport.Add($"{ClassicScanLogsVariables.ClassicVersion} | {ClassicScanLogsVariables.ClassicVersionDate} | END OF AUTOSCAN \n");

                statsCrashlogScanned++;
                if (triggerScanFailed)
                {
                    scanFailedList.Add(Path.GetFileName(crashlogFile));
                }

                // HIDE PERSONAL USERNAME
                for (int i = 0; i < autoscanReport.Count; i++)
                {
                    autoscanReport[i] = autoscanReport[i]
                        .Replace($"{Path.GetDirectoryName(userFolder)}\\{Path.GetFileName(userFolder)}", "******")
                        .Replace($"{Path.GetDirectoryName(userFolder)}/{Path.GetFileName(userFolder)}", "******");
                }

                // WRITE AUTOSCAN REPORT TO FILE
                string autoscanName = Path.ChangeExtension(crashlogFile, null) + "-AUTOSCAN.md";
                File.WriteAllLines(autoscanName, autoscanReport, Encoding.UTF8);
                Console.WriteLine($"- - -> RUNNING CRASH LOG FILE SCAN >>> SCANNED {Path.GetFileName(crashlogFile)}");

                if (triggerScanFailed && YamlData.Settings_Query<bool>("Move Unsolved"))
                {
                    string backupPath = Path.Combine("CLASSIC Backup", "Unsolved Logs");
                    Directory.CreateDirectory(backupPath);
                    string crashMove = Path.Combine(backupPath, Path.GetFileName(crashlogFile));
                    string scanMove = Path.Combine(backupPath, Path.GetFileName(autoscanName));

                    if (File.Exists(crashlogFile))
                    {
                        File.Copy(crashlogFile, crashMove, true);
                    }
                    if (File.Exists(autoscanName))
                    {
                        File.Copy(autoscanName, scanMove, true);
                    }
                }
            }

            // CHECK FOR FAILED OR INVALID CRASH LOGS
            if (scanFailedList.Any() || scanInvalidList.Any())
            {
                Console.WriteLine("❌ NOTICE : CLASSIC WAS UNABLE TO PROPERLY SCAN THE FOLLOWING LOG(S):");
                foreach (var failedLog in scanFailedList)
                {
                    Console.WriteLine(failedLog);
                }
                foreach (var invalidLog in scanInvalidList)
                {
                    Console.WriteLine(Path.GetFileName(invalidLog));
                }
                Console.WriteLine("===============================================================================");
                Console.WriteLine("Most common reason for this are logs being incomplete or in the wrong format.");
                Console.WriteLine("Make sure that your crash log files have the .log file format, NOT .txt! \n");
            }

            // CRASH LOG SCAN COMPLETE / TERMINAL OUTPUT
            Console.WriteLine("SCAN COMPLETE! (IT MIGHT TAKE SEVERAL SECONDS FOR SCAN RESULTS TO APPEAR)");
            Console.WriteLine("SCAN RESULTS ARE AVAILABLE IN FILES NAMED crash-date-and-time-AUTOSCAN.md \n");
            Console.WriteLine($"{GetRandomHint()}\n-----");
            Console.WriteLine($"Scanned all available logs in {scanStartTime.Elapsed.TotalSeconds:F2} seconds.");
            Console.WriteLine($"Number of Scanned Logs (No Autoscan Errors): {statsCrashlogScanned}");
            Console.WriteLine($"Number of Incomplete Logs (No Plugins List): {statsCrashlogIncomplete}");
            Console.WriteLine($"Number of Failed Logs (Autoscan Can't Scan): {statsCrashlogFailed}\n-----");
            if (Globals.Game == "Fallout4")
            {
                Console.WriteLine(ClassicScanLogsVariables.AutoscanText);
            }
            if (statsCrashlogScanned == 0 && statsCrashlogIncomplete == 0)
            {
                Console.WriteLine("\n❌ CLAS found no crash logs to scan or the scan failed.");
                Console.WriteLine("    There are no statistics to show (at this time).\n");
            }
        }
        private static bool DetectModsSingle(Dictionary<string, string> yamlDict, Dictionary<string, string> crashlogPlugins, List<string> autoscanReport)
        {
            bool triggerModFound = false;
            foreach (var (modWarn, plugin) in from kvp in yamlDict
                                              let modName = kvp.Key
                                              let modWarn = kvp.Value
                                              from plugin in crashlogPlugins
                                              where plugin.Key.ToLower().Contains(modName.ToLower())
                                              select (modWarn, plugin))
            {
                autoscanReport.AddRange(new[] { $"[!] FOUND : [{plugin.Value}] ", modWarn });
                triggerModFound = true;
                break;
            }

            return triggerModFound;
        }

        private static bool DetectModsDouble(Dictionary<string, string> yamlDict, Dictionary<string, string> crashlogPlugins, List<string> autoscanReport)
        {
            bool triggerModFound = false;
            foreach (var kvp in yamlDict)
            {
                string modName = kvp.Key;
                string modWarn = kvp.Value;
                string[] modSplit = modName.Split(" | ", 2);

                if (modSplit.Length != 2) continue; // Skip if the split didn't result in two parts

                bool mod1Found = false;
                bool mod2Found = false;

                foreach (var plugin in crashlogPlugins.Keys)
                {
                    if (plugin.Contains(modSplit[0], StringComparison.OrdinalIgnoreCase))
                    {
                        mod1Found = true;
                    }
                    if (plugin.Contains(modSplit[1], StringComparison.OrdinalIgnoreCase))
                    {
                        mod2Found = true;
                    }
                    if (mod1Found && mod2Found)
                    {
                        break;
                    }
                }

                if (mod1Found && mod2Found)
                {
                    autoscanReport.AddRange(new[] { "[!] CAUTION : ", modWarn });
                    triggerModFound = true;
                }
            }
            return triggerModFound;
        }

        private static void DetectModsImportant(Dictionary<string, string> yamlDict, Dictionary<string, string> crashlogPlugins, List<string> autoscanReport, bool crashlogGPUAMD, bool crashlogGPUNV, bool crashlogGPUI)
        {
            string? gpuRival = crashlogGPUAMD || crashlogGPUI ? "nvidia" : crashlogGPUNV ? "amd" : null;
            foreach (var (modWarn, modSplit, modFound) in from kvp in yamlDict
                                                          let modName = kvp.Key
                                                          let modWarn = kvp.Value
                                                          let modSplit = modName.Split(" | ", 2)
                                                          let modFound = crashlogPlugins.Keys.Any(plugin => plugin.ToLower().Contains(modSplit[0].ToLower()))
                                                          select (modWarn, modSplit, modFound))
            {
                if (modFound)
                {
                    if (gpuRival != null && modWarn.Contains(gpuRival, StringComparison.CurrentCultureIgnoreCase))
                    {
                        autoscanReport.AddRange(new[]
                        {
                            $"❓ {modSplit[1]} is installed, BUT IT SEEMS YOU DON'T HAVE AN {gpuRival.ToUpper()} GPU?\n",
                            "IF THIS IS CORRECT, COMPLETELY UNINSTALL THIS MOD TO AVOID ANY PROBLEMS! \n\n"
                        });
                    }
                    else
                    {
                        autoscanReport.Add($"✔️ {modSplit[1]} is installed!\n\n");
                    }
                }
                else
                {
                    if (gpuRival == null || !modWarn.ToLower().Contains(gpuRival))
                    {
                        autoscanReport.AddRange(new[] { $"❌ {modSplit[1]} is not installed!\n", modWarn, "\n" });
                    }
                }
            }
        }

        private static string GetRandomHint()
        {
            var hints = ClassicScanLogsVariables.Hints!.Split('\n');
            return hints[random.Next(hints.Length)];
        }
    }





}