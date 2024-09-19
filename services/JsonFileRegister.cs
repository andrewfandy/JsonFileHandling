
namespace JsonFileHandling;

public static class JsonFileRegister
{
    public static List<JsonFile> RegisterManyFiles(string filePath)
    {
        List<JsonFile> jsonFiles = new List<JsonFile>();
        try
        {
            string[] files = Directory.GetFiles(filePath);
            foreach (string file in files)
            {
                if (!Validation.FilePathIsValid(file)) continue;
                jsonFiles.Add(new JsonFile(file));
            }
        }
        catch (DirectoryNotFoundException de)
        {
            Console.WriteLine(de.Message);
        }
        catch (FileNotFoundException fe)
        {
            Console.WriteLine(fe.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return jsonFiles;
    }
    public static JsonFile RegisterSingleFile(string filePath)
    {
        JsonFile json = null!;
        if (Validation.FilePathIsValid(filePath)) json = new JsonFile(filePath);
        return json;
    }
}