namespace CLASSICCore
{
    public class DocsPathGenerator
    {
        public static void DocsGeneratePaths()
        {
            Console.WriteLine("- - - INITIATED DOCS PATH GENERATION");

            string? xseAcronym = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.XSE_Acronym");
            string? xseAcronymBase = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game_Info.XSE_Acronym");
            string? docsPath = YamlData.CLASSIC_Fallout4.ReadEntry<string>($"Game{Globals.Vr}_Info.Root_Folder_Docs");

#pragma warning disable CS8604 // Possible null reference argument.
            YamlData.CLASSIC_Fallout4_Local.UpdateEntry($"Game{Globals.Vr}_Info.Docs_Folder_XSE", Path.Combine(docsPath, xseAcronymBase));

            YamlData.CLASSIC_Fallout4_Local.UpdateEntry($"Game{Globals.Vr}_Info.Docs_File_PapyrusLog", Path.Combine(docsPath, "Logs", "Script", "Papyrus.0.log"));
            YamlData.CLASSIC_Fallout4_Local.UpdateEntry($"Game{Globals.Vr}_Info.Docs_File_WryeBashPC", Path.Combine(docsPath, "ModChecker.html"));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            YamlData.CLASSIC_Fallout4_Local.UpdateEntry($"Game{Globals.Vr}_Info.Docs_File_XSE", Path.Combine(docsPath, xseAcronymBase, $"{xseAcronym.ToLower()}.log"));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}