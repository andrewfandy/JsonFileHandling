using JsonFileHandling;

namespace model;

public static class JsonFileRegister
{
    public static JsonFile? RegisterFile(string path)
    {
        if (JsonExistsInPath(path)) return new JsonFile(path);

        return null;
    }

    public static List<JsonFile?> RegisterFiles(string path)
    {
        List<JsonFile?> jsonFiles = new List<JsonFile?>();
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            jsonFiles.Add(RegisterFile(file));
        }
        return jsonFiles;
    }


    private static bool JsonExistsInPath(string path)
    {
        try
        {
            if (Path.GetFileName(path).EndsWith(".json") && File.Exists(path)) return true;


            var files = Directory.GetFiles(path);
            if (files.Any(file => file.EndsWith(".json"))) return true;

            throw new FileNotFoundException();
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"\nDirectory {path} not found\n");
            return false;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("\nJSON File not found\n");
            return false;
        }
    }

}