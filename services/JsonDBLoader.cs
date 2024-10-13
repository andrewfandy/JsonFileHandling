
using System.Text;
using System.Text.Json;
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


public class JsonDBLoader
{
    /*
        TODO:
    - Implement HttpClient request for testing the processed JSON file
    - Host : http://localhost:5249
    - POST users/login
        body:
        - user: string@gmail.com
        - password: string
        - response: token
    - Store token locally
    
    - iteration_1 start
    - Manually input policy (EQY: 1, SBX: 2)

    - iteration_2 start
    - POST certificates/
        header:
        - Authorization: Bearer ACCESS_TOKEN
        body:
        - PolicyId
        - CurrencyId
        - ... any processed JObject's field
    - iteration_2 end
    - iteration_1 end
    
    */

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

            using var response = await _client.PostAsync("/api/users/login", content);
            string message = await response.Content.ReadAsStringAsync();

            var serialize = JObject.Parse(message);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Login Success");



                token = serialize["token"]?.ToString();

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            }
            else
            {
                Console.WriteLine($"Error:\n{serialize["Message"]?.ToString()}");
            }
            Console.WriteLine(token);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"REQUEST ERROR");
            return;
        }

    }


    public async Task TryInputCertificate()
    {
        foreach (var json in _jsonObject)
        {
            string id = json.Key;
            Console.WriteLine($"Inputting {id}");

            var obj = (JObject)json.Value!;
            bool confirmed = obj.Value<bool>("isConfirmed");
            obj.Remove("isConfirmed");
            obj.Remove("isReported");
            obj["userEmail"] = _user;


            var payload = obj.ToString();

            Console.WriteLine(payload);
            using StringContent content = new StringContent(
                        payload,
                        Encoding.UTF8,
                        "application/json"
                        );

            using var response = await _client.PostAsync("/api/certificates/", content);
            var message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"{id} succeed");
                if (confirmed)
                {
                    var responseMessage = JObject.Parse(message);
                    int certId = responseMessage.Value<int>("Id");
                    Console.WriteLine($"Confirming {certId}");
                    await TryIssued(certId);
                };
            }
            else
            {
                var error = JObject.Parse(message);
                Console.WriteLine($"Error: {error.ToString()}");
                break;
            }
        }
    }

    public async Task TryUpdateLCNumber()
    {
        foreach (var obj in _jsonObject)
        {
            var certNumber = obj.Key;
            var lcNumber = obj.Value!.ToString();



            var getByCertNumber = await _client.GetAsync($"api/certificates/certNumber?certNumber={certNumber}&email={_user}&page={1}&pageSize={10}");
            var message = JsonDocument.Parse(await getByCertNumber.Content.ReadAsStreamAsync());
            var root = message.RootElement;

            var data = root.GetProperty("data").EnumerateArray();
            foreach (var certificates in data)
            {
                Console.WriteLine(certificates);
                // var json = JsonSerializer.Serialize(new {
                //     id = certificates.GetProperty("id"),

                // })
                // var payload = new StringContent(
                //     json,
                //     Encoding.UTF8,
                //     "application/json"
                // );
            }



        }
    }

    private async Task TryIssued(int id)
    {
        var payload = JsonSerializer.Serialize(new { certId = id });

        var content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json"
        );
        using var response = await _client.PutAsync("/api/certificates/issue", content);
        var message = await response.Content.ReadAsStringAsync();

        Console.WriteLine(message);

        response.EnsureSuccessStatusCode();
    }
}