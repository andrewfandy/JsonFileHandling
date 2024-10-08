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


    public async Task TryInputCertificate()
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