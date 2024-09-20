
namespace JsonFileHandling;

public static class JsonFileRegister
{
    public static List<JsonFile> RegisterManyFiles(string filePath)
    {
        List<JsonFile> jsonFiles = null!;
        try
        {
            jsonFiles = new List<JsonFile>();
            string[] files = Directory.GetFiles(filePath);
            foreach (string file in files)
            {
                if (!Validation.IsJsonFile(file)) continue;
                jsonFiles.Add(new JsonFile(file));
            }

            if (files.Count() < 1) throw new Exception("JSON File not found");
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
        if (Validation.IsJsonFile(filePath)) json = new JsonFile(filePath);
        return json;
    }
}