using System.Net.Http;
using HtmlAgilityPack;
namespace CLASSICCore
{
    public class MainFilesBackup
    {
        private static readonly HttpClientHandler Handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        private static readonly HttpClient Client = new HttpClient(Handler);

    public static async Task MainFilesBackupAsync()
    {
        var backupList = YamlData.CLASSIC_Main.ReadEntry<List<string>>("CLASSIC_AutoBackup");
        var gamePath = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string>($"Game{Globals.Vr}_Info.Root_Folder_Game");
        var xseAcronym = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.XSE_Acronym");
        var xseAcronymBase = YamlData.CLASSIC_Fallout4.ReadEntry<string>("Game_Info.XSE_Acronym");
        var xseLogFile = YamlData.CLASSIC_Fallout4_Local.ReadEntry<string>($"Game{Globals.Vr}_Info.Docs_File_XSE");
        var xseVerLatest = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.XSE_Ver_Latest");

#pragma warning disable CS8604 // Possible null reference argument.
        var xseData = File.ReadAllLines(xseLogFile);
#pragma warning restore CS8604 // Possible null reference argument.
        var versionXse = xseVerLatest;

        foreach (var line in xseData)
        {
            if (line.Contains("version = ", StringComparison.CurrentCultureIgnoreCase))
            {
                var splitXse = line.Split(' ');
                var index = Array.FindIndex(splitXse, item => item.Contains("version", StringComparison.CurrentCultureIgnoreCase)) + 2;
                versionXse = splitXse[index];
                break;
            }
        }

        var backupPath = $"CLASSIC Backup/Game Files/{versionXse}";
        Directory.CreateDirectory(backupPath);

#pragma warning disable CS8604 // Possible null reference argument.
            var gameFiles = Directory.GetFiles(gamePath);
#pragma warning restore CS8604 // Possible null reference argument.
            var backupFiles = Directory.GetFiles(backupPath);

        foreach (var file in gameFiles)
        {
            var fileName = Path.GetFileName(file);
#pragma warning disable CS8604 // Possible null reference argument.
                if (backupList.Any(item => item == fileName) && !backupFiles.Any(item => Path.GetFileName(item) == fileName))
            {
                var destinationFile = Path.Combine(backupPath, fileName);
                File.Copy(file, destinationFile, true);
            }
#pragma warning restore CS8604 // Possible null reference argument.
            }

        var xseLinks = new List<string>();
        try
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var response = await Client.GetAsync($"https://{xseAcronymBase.ToLower()}.silverlock.org");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);
                var links = doc.DocumentNode.SelectNodes("//a[@href]");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        var href = link.GetAttributeValue("href", string.Empty);
                        if (!string.IsNullOrEmpty(href) && (href.EndsWith(".7z") || href.EndsWith(".zip")))
                        {
                            xseLinks.Add(href);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"❌ ERROR : Unable to check for {xseAcronym} updates. \n Status Code: {response.StatusCode} \n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR : Unable to check for {xseAcronym} updates. \n {ex.Message} \n");
        }

        if (xseLinks.Any())
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var versionFormat = versionXse.Replace(".", "_").Replace("0_", "");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (!xseLinks.Any(link => link.Contains(versionFormat)))
            {
                var warningMessage = YamlData.CLASSIC_Fallout4.ReadEntry<string>("Warnings_XSE.Warn_Outdated");
                Console.WriteLine(warningMessage);
            }
        }
    }
}
}