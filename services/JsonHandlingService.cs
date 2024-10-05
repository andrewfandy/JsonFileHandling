using System.Text;
using JsonFileHandling;
using Newtonsoft.Json.Linq;
using utils;

namespace services;

public static class JsonHandlingService
{

    /// <summary>
    /// Compare two JObject objects. If there's unmatch data between both, the method would return the collection of unmatched object
    /// </summary>
    /// <param name="comparer"></param>
    /// <returns>List<JObject></returns>
    public static JObject? CompareAndMerge(JsonFile? draft, JsonFile? confirmed)
    {
        if (draft == null || confirmed == null) return null;
        Dictionary<string, JObject?> result = new Dictionary<string, JObject?>();
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
    private static JToken SeparateNewToken(JToken token, string oldKey, string firstNewKey, string secondNewKey)
    {
        var oldValues = token.Value<string>(oldKey)!.Split();
        var firstNewVal = oldValues[0].Trim();
        var secondNewVal = oldValues[1].Trim();
        token[firstNewKey] = firstNewVal;


        token[secondNewKey] = DateTime.ParseExact(secondNewVal, GetDateTimeFormat(secondNewVal.Trim()), System.Globalization.CultureInfo.InvariantCulture);

        return token;

    }

    /*
        TODO:
        - Fix index outbounds in line 69
        - Apply JSON Serialize to a file
    */
    public static JObject CompareFieldsOnDB(JObject jsonObject)
    {

        var result = new Dictionary<string, JObject?>();
        int PolicyId = InputHelper.InputToInt($"Please input PolicyID: ");
        foreach (var obj in jsonObject)
        {
            var certificate = obj.Key;
            Console.WriteLine($"COMPARE: {certificate}"); // REMOVE LATER

            var content = (JObject?)obj.Value;


            content!["PolicyId"] = PolicyId;
            content.Remove("TheInsured");

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

            result[certificate] = content;
            Console.WriteLine("STATUS: COMPLETED\n"); // REMOVE LATER

        }

        return JObject.FromObject(result);
    }
}