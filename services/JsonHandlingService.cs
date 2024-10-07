using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using JsonFileHandling;
using Newtonsoft.Json.Linq;
using utils;

namespace services;

public static class JsonHandlingService
{


    public static void SerializeJSON(JObject json, string outputDir, string? fileName = "output.json")
    {
        try
        {

            File.WriteAllText(Path.Combine(outputDir, fileName!), json.ToString());
        }
        catch (DirectoryNotFoundException)
        {
            throw new UnauthorizedAccessException();
        }
        catch (UnauthorizedAccessException)
        {
            string output = InputHelper.Input("DIRECTORY UNACCESSIBLE, PLEASE INPUT AGAIN: ");
            SerializeJSON(json, output, fileName);
        }

    }

    /// <summary>
    /// Compare two JObject objects. If there's unmatch data between both, the method would return the collection of unmatched object
    /// </summary>
    /// <param name="comparer"></param>
    /// <returns>List<JObject></returns>
    public static JObject? CompareAndMerge(JsonFile? draft, JsonFile? confirmed)
    {
        if (draft == null || confirmed == null) return null;
        var result = new JObject();
        foreach (var obj in draft.JsonObject)
        {
            var key = obj.Key;
            var val = (JObject?)obj.Value;

            if (!val!.ContainsKey("Transhipment"))
            {
                val["Transhipment"] = null;
            }

            result[key] = val;
        }

        foreach (var obj in confirmed.JsonObject)
        {
            var key = obj.Key;
            var val = (JObject?)obj.Value;
            // Revise the value
            val!["IsConfirmed"] = true;
            result[key] = val;
        }

        return result;
    }

    /// <summary>
    /// Customized compare and restructure the JSON so match to the database
    /// </summary>
    /// <param name="jsonObject">a JSON Object</param>
    /// <returns>JObject</returns>
    public static JObject CompareFieldsOnDB(JObject jsonObject)
    {

        var result = new JObject();
        int PolicyId = InputHelper.InputToInt($"Please input PolicyID: ");
        foreach (var obj in jsonObject)
        {
            var certificate = obj.Key;
            var content = (JObject?)obj.Value;


            content!["PolicyId"] = PolicyId;
            content.Remove("TheInsured");
            content.Remove("Address");

            var CurrencyId = GetCurrencyId(content.Value<string>("Currency")!);
            content["CurrencyId"] = CurrencyId;
            content.Remove("Currency");

            SeparateNewToken(content, "BLNo", "BLNumber", "BLNumberDate");
            content.Remove("BLNo");

            SeparateNewToken(content, "InvoiceNo", "InvoiceNumber", "InvoiceNumberDate");
            content.Remove("InvoiceNo");

            content["LCNumber"] = content["DocumentaryCreditNo"];
            content.Remove("DocumentaryCreditNo");

            content["SailingDate"] = content["DateOfDeparture"];
            content.Remove("DateOfDeparture");

            content.Remove("Deductible");
            content.Remove("Conditions");

            result.Add(certificate, content);

        }

        return JObject.FromObject(result);
    }
    private static int GetCurrencyId(string val)
    {
        var currencyKeyValuePairs = new Dictionary<string, int>
        {
            {"EUR", 1}, {"USD", 2}, {"GBP", 3}, {"JPY", 4}, {"AUD", 5}, {"CAD", 6},
            {"CHF", 7}, {"CNY", 8}, {"INR", 9}, {"IDR", 10}, {"BRL", 11}, {"MXN", 12},
            {"RUB", 13}, {"ZAR", 14}, {"KRW", 15}, {"SGD", 16}, {"NZD", 17}, {"TRY", 18},
            {"SEK", 19}, {"NOK", 20}, {"DKK", 21}, {"PLN", 22}, {"THB", 23}, {"MYR", 24}
        };


        if (currencyKeyValuePairs.ContainsKey(val))
        {
            return currencyKeyValuePairs[val];
        }
        return 0;

    }

    private static string GetDateTimeFormat(string str)
    {

        var split = str.Split("-");
        StringBuilder sb = new StringBuilder();
        sb.Append('d', split[0].Length);
        sb.Append('-');
        sb.Append('M', split[1].Length);
        sb.Append('-');
        sb.Append('y', split[2].Length);
        return sb.ToString();

    }
    private static void SeparateNewToken(JToken token, string oldKey, string firstNewKey, string secondNewKey, string? format = null)
    {

        var oldValue = token.Value<string?>(oldKey);

        string pattern = @"\b(\d{1,2}-(\d{1,2}|\w{3})-(\d{2}|\d{4}))\b";
        Regex r = new Regex(pattern);
        MatchCollection m = r.Matches(token.Value<string?>(oldKey)!);

        string firstNewVal, secondNewVal;
        if (m.Count < 1)
        {
            token[firstNewKey] = oldValue;
            token[secondNewKey] = null;
            return;
        }

        var oldValues = oldValue?.Split(",");

        firstNewVal = oldValues![0];
        secondNewVal = m[0].Value;



        // trying to add secondNewKey
        try
        {
            if (format == null)
            {
                format = GetDateTimeFormat(secondNewVal!.Trim());
            }
            token[firstNewKey] = firstNewVal;
            token[secondNewKey] = DateTime.ParseExact(secondNewVal!, format, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            var cultureInfo = CultureInfo.CurrentCulture;
            Console.WriteLine($"$INVALID FORMAT OF {secondNewVal}\nNative Culture's Name: {cultureInfo.NativeName} or {cultureInfo.Name}");
            string newFormat = InputHelper.Input();
            SeparateNewToken(token, firstNewKey, secondNewKey, newFormat);
        }


    }

    /*
        TODO:
        - Apply JSON Serialize to a file
    */

}