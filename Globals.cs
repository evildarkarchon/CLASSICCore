namespace CLASSICCore
{
    public class Globals
    {
        public static string? Game
        {
            get
            {
                return "Fallout4";
            }
        }
        public static string Vr
        {
            get
            {
                switch (YamlData.Settings_Query<bool>("VR Mode"))
                {
                    case true:
                        return "vr";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}