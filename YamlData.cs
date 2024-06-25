using Yaml;

namespace CLASSICCore
{
    public class YamlData
    {
        public static YamlCache CLASSIC_Main { get; set; } = new YamlCache("CLASSIC Data/databases/CLASSIC Main.yaml");
        public static YamlCache CLASSIC_Fallout4 { get; set; } = new YamlCache("CLASSIC Data/databases/CLASSIC Fallout4.yaml");
        public static YamlCache CLASSIC_Settings { get; set; } = new YamlCache("CLASSIC Settings.yaml");
        public static YamlCache CLASSIC_Ignore { get; set; } = new YamlCache("CLASSIC Ignore.yaml");
        public static YamlCache CLASSIC_Fallout4_Local { get; set; } = new YamlCache("CLASSIC Data/CLASSIC Fallout4 Local.yaml");

        public static string? Settings_Query(string key)
        {
            if (!File.Exists("CLASSIC Settings.yaml"))
            {
                using StreamWriter sw = File.CreateText("CLASSIC Settings.yaml");
                sw.Write(CLASSIC_Main.ReadEntry<string>("default_settings"));
            }
            string? value = CLASSIC_Settings.ReadEntry<string>(key);
            return value;
        }
        public static T? Settings_Query<T>(string key)
        {
            if (!File.Exists("CLASSIC Settings.yaml"))
            {
                using StreamWriter sw = File.CreateText("CLASSIC Settings.yaml");
                sw.Write(CLASSIC_Main.ReadEntry<string>("default_settings"));
            }
            return CLASSIC_Settings.ReadEntry<T>(key);
        }
    }
}