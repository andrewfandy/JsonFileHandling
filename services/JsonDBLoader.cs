
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace services;


public class InsurancePolicy
{
    public int Id { get; set; }
    public string CertNumber { get; set; }
    public string UserEmail { get; set; }
    public string PolicyName { get; set; }
    public string Mop { get; set; }
    public string CurrencyName { get; set; }
    public decimal InsuredValue { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CertificateDetailViewModel
{
    public int Id { get; set; }
    public string CertNumber { get; set; }
    public string UserEmail { get; set; }
    public int PolicyId { get; set; }
    public int CurrencyId { get; set; }
    public string? Consignee { get; set; }
    public string Conveyance { get; set; }
    public DateTime SailingDate { get; set; }
    public decimal InsuredValue { get; set; }
    public string InterestInsured { get; set; }
    public string? BLNumber { get; set; }
    public DateTime? BLNumberDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceNumberDate { get; set; }
    public string? LCNumber { get; set; }
    public string? Transhipment { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime LastModified { get; set; }
}


public class JsonDBLoader
{


    private readonly JObject _jsonObject;
    private readonly HttpClient _client;
    private readonly string _user;
    private readonly string _password;
    public string? token { get; private set; }


    public JsonDBLoader(JObject jsonObject, string host, string user, string password)
    {
        _jsonObject = jsonObject;
        _client = new HttpClient { BaseAddress = new Uri(host) };
        _user = user;
        _password = password;
    }
    public JsonDBLoader(Dictionary<string, string> obj, string host, string user, string password)
    {
        _jsonObject = JObject.FromObject(obj);
        _client = new HttpClient { BaseAddress = new Uri(host) };
        _user = user;
        _password = password;
    }

    public async Task TryLoginAsync()
    {
        try
        {
            using StringContent content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    email = _user,
                    password = _password
                }),
                Encoding.UTF8,
                "application/json"
                );

            using var request = await _client.PostAsync("/api/users/login", content);
            string response = await request.Content.ReadAsStringAsync();
            var message = JObject.Parse(response);

            if (request.IsSuccessStatusCode)
            {
                Console.WriteLine("Login Success");
                
                token = message.Value<string>("token");

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            }
            else
            {
                var error = message.Value<string>("message");
                Console.WriteLine(new { message = error });
            }
            Console.WriteLine(token);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"REQUEST ERROR");
            return;
        }

    }


    public async Task TryPostCertificate()
    {
        foreach (var json in _jsonObject)
        {
            string id = json.Key;
            var obj = (JObject)json.Value!;
            bool confirmed = obj.Value<bool>("isConfirmed");
            obj.Remove("isConfirmed");
            obj.Remove("isReported");
            obj["userEmail"] = _user;


            var payload = obj.ToString();

            using StringContent content = new StringContent(
                        payload,
                        Encoding.UTF8,
                        "application/json"
                        );

            using var request = await _client.PostAsync("/api/certificates/", content);
            var response = await request.Content.ReadAsStringAsync();
            var message = JObject.Parse(response);

            if (request.IsSuccessStatusCode)
            {
                Console.WriteLine($"{id} inputted");
                if (confirmed)
                {
                    int certId = message.Value<int>("id");
                    await TryIssued(certId);
                };
            }
            else
            {
                var error = message.Value<string>("message");
                Console.WriteLine($"Error: {message}");
                break;
            }
        }
    }

    public async Task TryPutCertificate()
    {
        foreach (var obj in _jsonObject)
        {
            var certNumber = obj.Key;
            var lcNumber = obj.Value!.ToString();

            using var response = await _client.GetAsync($"api/certificates/certNumber?certNumber={certNumber}&email={_user}&page={1}&pageSize={10}");
            var message = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
            var root = message.RootElement;

            var certificates = root.GetProperty("data").EnumerateArray();
            foreach (var certificate in certificates)
            {
                TryUpdateLCNumber(certificate, lcNumber);
            }
        }
    }

    private async Task TryUpdateLCNumber(JsonElement certificate, string lcNumber)
    {
        var id = certificate.GetProperty("id");
        using var get = await _client.GetAsync($"api/certificates/certificate/{id}?email={_user}");
        var element = JsonDocument.Parse(await get.Content.ReadAsStringAsync()).RootElement;

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var cert =
            JsonSerializer.Deserialize<CertificateDetailViewModel>(element,options);

        cert.LCNumber = lcNumber;

        var newCertJson = JsonSerializer.Serialize(cert);
        var payload = new StringContent(
            newCertJson,
            Encoding.UTF8,
            "application/json"
        );

        using var put = await _client.PutAsync("api/certificates/",payload);
        var message = JsonDocument.Parse(await put.Content.ReadAsStringAsync());
        if (!put.IsSuccessStatusCode)
        {
            var errorMsg = message.RootElement.GetProperty("message").GetString();
            Console.WriteLine($"{cert.CertNumber}'s LC number failed\nMessage: {errorMsg}");
        }
    }
    private async Task TryIssued(int id)
    {
        var uri = $"api/certificates/issue?certId={id}";
        using var request = await _client.PutAsync(uri, null);
        var response = await request.Content.ReadAsStringAsync();
        var message = JObject.Parse(response);

        if (!request.IsSuccessStatusCode)
        {
            var error = message.Value<string>("message");
            Console.WriteLine(new { message = error });
            request.EnsureSuccessStatusCode();
        }
    }
}