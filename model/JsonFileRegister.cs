using JsonFileHandling;

namespace model;

public static class JsonFileRegister
{
    public static JsonFile RegisterFile(string path)
    {
        return new JsonFile(path);
    }

    public static List<JsonFile> RegisterFiles(string path)
    {
        List<JsonFile> jsonFiles = new List<JsonFile>();
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            jsonFiles.Add(RegisterFile(file));
        }
        return jsonFiles;
    }
}