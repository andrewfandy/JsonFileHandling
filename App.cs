using System.Diagnostics;

namespace JsonFileHandling;


public class App
{
    public App()
    {
        Console.WriteLine("=== JSON FILE HANDLER APP ===");
        Run();
    }


    private void Run()
    {
        string pathConfirmed = "";
        string pathDraft = "";
        while (string.IsNullOrEmpty(pathConfirmed))
        {
            Console.WriteLine("Input path of confirmed doc: ");
            pathConfirmed = Console.ReadLine()!;
        }
        while (string.IsNullOrEmpty(pathDraft))
        {

            Console.WriteLine("Input path of draft doc: ");
            pathDraft = Console.ReadLine()!;
        }
        if (File.Exists(pathConfirmed) && File.Exists(pathDraft))
        {
            var confirmed = JsonFileRegister.RegisterSingleFile(pathConfirmed);
            var draft = JsonFileRegister.RegisterSingleFile(pathDraft);

            if (confirmed != null && draft != null)
            {
                Console.WriteLine("Processing...");
                ProcessSingleFile(confirmed, draft);
            }
        }
        else if (Directory.Exists(pathConfirmed) && Directory.Exists(pathDraft))
        {
            var confirmed = JsonFileRegister.RegisterManyFiles(pathConfirmed);
            var draft = JsonFileRegister.RegisterManyFiles(pathDraft);
            if (confirmed != null && draft != null)
            {
            }
        }
        else
        {
            Console.WriteLine("Either the JSON File(s) or Path(s) doesn't exists");
            Run();
        }
    }
    private void ProcessSingleFile(JsonFile confirmedFile, JsonFile draftFile)
    {

    }
    private void ProcessingManyFiles(List<JsonFile> confirmedFiles, List<JsonFile> draftFiles)
    {
        // foreach()
        // Console.WriteLine($"Processing {confirmedFile.FilePath} and {draftFile.FilePath}");
    }
}