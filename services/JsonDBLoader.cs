using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace services;

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

            Console.WriteLine(obj.ToString());

            var payload = JsonSerializer.Serialize(obj);
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
                var status = error["status"];
                var errors = error["errors"];
                Console.WriteLine($"Error\nStatus:{status}\nErrors occured: {errors}");
                break;
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