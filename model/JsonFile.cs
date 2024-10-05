using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JsonFileHandling;


public class JsonFile
{
    /// <summary>
    /// A file's path of the JSON
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// Get list of properties inside the JSON File
    /// </summary>
    public List<JProperty> Properties { get; private set; }



    /// <summary>
    /// Return a List of JObject inside JSON FIle
    /// </summary>
    public JObject JsonObject { get; private set; }



    /// <summary>
    /// Instantiate JsonFile through a file in directory
    /// </summary>
    /// <param name="path">Requires valid path</param>
    /// <exception cref="IOException"/>
    /// <exception cref="Newtonsoft.Json.JsonReaderException"/>
    public JsonFile(string path)
    {
        try
        {
            FilePath = path;
            string json = File.ReadAllText(FilePath);
            JsonObject = JObject.Parse(json); // deserializing the file to jsonobject
            Properties = JsonObject.Properties().ToList();
        }
        catch (IOException)
        {
            Console.WriteLine($"ERROR READ FILE {FilePath}");
            throw;
        }
        catch (Newtonsoft.Json.JsonReaderException)
        {
            Console.WriteLine($"INVALID JSON FORMAT IN {FilePath}");
            throw;
        }
    }

    /// <summary>
    /// Instantiate JsonFile with existing JObject
    /// </summary>
    /// <param name="jsonObject">Require JObject</param>
    /// <param name="path">Requires valid path</param>
    /// <exception cref="DirectoryNotFoundException">Throw FileNotFoundException if path is not found</exception>
    /// <exception cref="FileNotFoundException">Throw FileNotFoundException if path is not found</exception>
    public JsonFile(JObject jsonObject, string path)
    {
        try
        {
            FilePath = path;
            JsonObject = jsonObject;
            Properties = JsonObject.Properties().ToList();

        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"Directory in {path} not found");
            throw;
        }
    }
}
