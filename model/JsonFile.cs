using Newtonsoft.Json.Linq;

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
    /// Get <returns>JObject</returns>
    /// </summary>
    public JObject JsonObject { get; private set; }
    // required public JToken token { get; set; }
    public JsonFile(string path)
    {
        FilePath = path;
        string json = File.ReadAllText(FilePath);
        JsonObject = JObject.Parse(json); // deserializing the file to jsonobject
        Properties = JsonObject.Properties().ToList();
    }

}