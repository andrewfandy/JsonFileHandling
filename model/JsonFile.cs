using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonFileHandling;


public class JsonFile
{
    public string FilePath { get; private set; }
    // public JObject JsonObject { get; private set; }
    // required public JToken token { get; set; }
    public JsonFile(string Path)
    {
        FilePath = Path;
        // JsonObject = JsonConvert.DeserializeObject
    }

}