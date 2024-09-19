namespace JsonFileHandling;


public class App
{
    public App()
    {
        Run();
    }

    private void Run()
    {
        Console.WriteLine("=== JSON FILE HANDLER APP ===");
        string path = "";
        while (string.IsNullOrEmpty(path))
        {
            Console.WriteLine("Input folder path: ");
            path = Console.ReadLine()!;
        }
        if (File.Exists(path))
        {
            JsonFile confirmed = JsonFileRegister.RegisterSingleFile(path);
            JsonFile draft = JsonFileRegister.RegisterSingleFile(path);
            ProcessSingleFile(confirmed, draft);
        }
        else if (Directory.Exists(path))
        {
            List<JsonFile> files = JsonFileRegister.RegisterManyFiles(path);
            Console.WriteLine("Success");
            // ProcessingManyFiles(files);
        }
    }
    private void ProcessSingleFile(JsonFile confirmedFile, JsonFile draftFile)
    {
        Console.WriteLine($"Processing {confirmedFile.FilePath} and {draftFile.FilePath}");

    }
    private void ProcessingManyFiles(List<JsonFile> confirmedFiles, List<JsonFile> draftFiles)
    {
        // foreach()
        // Console.WriteLine($"Processing {confirmedFile.FilePath} and {draftFile.FilePath}");
    }
}