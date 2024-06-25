using System.Text.Json;

namespace CLASSICCore
{
    public class UpdateChecker
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<bool> ClassicUpdateCheck()
        {
            LoggerConfig.LogDebug("INITIATED UPDATE CHECK");
            if (YamlData.Settings_Query<bool>("Update Check") == true)
            {
                string? classicLocal = YamlData.CLASSIC_Main.ReadEntry<string>("CLASSIC_Info.version");
                Console.WriteLine("❓ (Needs internet connection) CHECKING FOR NEW CLASSIC VERSIONS...");
                Console.WriteLine("   (You can disable this check in CLASSIC Settings.yaml) \n");

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/evildarkarchon/CLASSIC-Fallout4/releases/latest");
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(jsonResponse);
                    string? classicVerReceived = jsonDocument.RootElement.GetProperty("name").GetString();

                    Console.WriteLine($"Your CLASSIC Version: {classicLocal}\nNewest CLASSIC Version: {classicVerReceived}\n");

                    if (classicVerReceived == classicLocal)
                    {
                        Console.WriteLine("✔️ You have the latest version of CLASSIC! \n");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine(YamlData.CLASSIC_Main.ReadEntry<string>($"CLASSIC_Interface.update_warning_{Globals.Game}"));
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(YamlData.CLASSIC_Main.ReadEntry<string>($"CLASSIC_Interface.update_unable_{Globals.Game}"));
                }
            }
            else
            {
                Console.WriteLine("\n❌ NOTICE: UPDATE CHECK IS DISABLED IN CLASSIC Settings.yaml \n");
                Console.WriteLine("===============================================================================");
            }
            return false;
        }
    }
}