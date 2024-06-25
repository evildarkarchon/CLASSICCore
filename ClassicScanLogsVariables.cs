namespace CLASSICCore
{
    public static class ClassicScanLogsVariables
    {
        public static string? Hints { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Game_Hints");
        public static string? CatchRecords { get; } = YamlData.CLASSIC_Main.ReadEntry("catch_log_records");
        public static string? ClassicVersion { get; } = YamlData.CLASSIC_Main.ReadEntry("CLASSIC_Info.version");
        public static string? ClassicVersionDate { get; } = YamlData.CLASSIC_Main.ReadEntry("CLASSIC_Info.version_date");
#pragma warning disable CS8604 // Possible null reference argument.
        public static string? CrashGenName { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Game_Info.CRASHGEN_LogName");
        public static string? CrashGenLatestOg { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Game_Info.CRASHGEN_LatestVer");
        public static string? CrashGenLatestVr { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("GameVR_Info.CRASHGEN_LatestVer");
        public static string? CrashGenIgnore { get; } = YamlData.CLASSIC_Fallout4.ReadEntry($"Game{Globals.Vr}_Info.CRASHGEN_Ignore");
        public static string? WarnNoPlugins { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Warnings_CRASHGEN.Warn_NOPlugins");
        public static string? WarnOutdated { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Warnings_CRASHGEN.Warn_Outdated");
        public static string? XseAcronym { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Game_Info.XSE_Acronym");
        public static string? GameIgnorePlugins { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Crashlog_Plugins_Exclude");
        public static string? GameIgnoreRecords { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Crashlog_Records_Exclude");
        public static string? SuspectsErrorList { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Crashlog_Error_Check");
        public static string? SuspectsStackList { get; } = YamlData.CLASSIC_Fallout4.ReadEntry("Crashlog_Stack_Check");
        public static string? AutoscanText { get; } = YamlData.CLASSIC_Main.ReadEntry($"CLASSIC_Interface.autoscan_text_{Globals.Game}");
        public static List<string>? RemoveList { get; } = YamlData.CLASSIC_Main.ReadEntry<List<string>>("exclude_log_records");
        public static string? IgnoreList { get; } = YamlData.CLASSIC_Ignore.ReadEntry($"CLASSIC_Ignore_{Globals.Game}");
        public static Dictionary<string, string>? GameModsConf { get; } = YamlData.CLASSIC_Fallout4.ReadEntry<Dictionary<string, string>>("Mods_CONF");
        public static Dictionary<string, string>? GameModsCore { get; } = YamlData.CLASSIC_Fallout4.ReadEntry<Dictionary<string, string>>("Mods_CORE");
        public static Dictionary<string, string>? GameModsFreq { get; } = YamlData.CLASSIC_Fallout4.ReadEntry<Dictionary<string, string>>("Mods_FREQ");
        public static Dictionary<string, string>? GameModsOpc2 { get; } = YamlData.CLASSIC_Fallout4.ReadEntry<Dictionary<string, string>>("Mods_OPC2");
        public static Dictionary<string, string>? GameModsSolu { get; } = YamlData.CLASSIC_Fallout4.ReadEntry<Dictionary<string, string>>("Mods_SOLU");
#pragma warning restore CS8604 // Possible null reference argument.
    }
}