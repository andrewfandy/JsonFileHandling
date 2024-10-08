
using System.Text.Json;
using model;
using Newtonsoft.Json.Linq;
using services;
using utils;

namespace JsonFileHandling;


public class App
{
    public App()
    {
        Console.WriteLine("=== JSON FILE HANDLER APP ===");
    }


    public async Task RunAsync()
    {
        var draftFile = JsonFileRegister.RegisterFile(InputHelper.Input("INPUT DRAFT .JSON FILE: "));

        var confirmedFile = JsonFileRegister.RegisterFile(InputHelper.Input("INPUT CONFIRMED .JSON FILE: "));

        var comparedJSON = JsonHandlingService.CompareAndMerge(draftFile, confirmedFile);

        if (comparedJSON == null)
        {
            Console.WriteLine("Compared JObject null");
            return;
        }
        var restructuredJSON = JsonHandlingService.RestructureJSON(comparedJSON);


        await LoadToDB(restructuredJSON);

        // LoadToJSONFile(restructuredJSON);
        // foreach (var obj in restructuredJSON)
        // {
        //     Console.WriteLine(obj.ToString());
        // }

        Console.WriteLine("\nProcess Ends\nStart again?press 'y' or 'n'");
        ConsoleKey key;
        do
        {
            key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Y)
            {
                new App();
            }
            else if (key == ConsoleKey.N)
            {
                Console.WriteLine("Good bye");
                return;
            }
        } while (key != ConsoleKey.Y || key != ConsoleKey.N);
    }

    private async Task LoadToDB(JObject? obj)
    {
        if (obj == null)
        {
            Console.WriteLine("JObject is null!");
            return;
        }
        string user = InputHelper.Input("EMAIL: ");
        string password = InputHelper.Input("PASSWORD: ");
        string host = "http://localhost:5249/";
        JsonDBLoader loader = new JsonDBLoader(obj, host, user, password);


        await loader.TryLoginAsync();
        if (string.IsNullOrEmpty(loader.token))
        {
            Console.WriteLine("LOGIN FAILED");
            return;
        }

        await loader.TryInputCertificate();

    }

    private void LoadToJSONFile(JObject? obj)
    {
        if (obj == null)
        {
            Console.WriteLine("JObject is null!");
            return;
        }
        string outDirectory = InputHelper.Input("INPUT OUTPUT DIR: ");
        string outputFileName = InputHelper.Input("INPUT OUTPUT FILE NAME: ");
        if (!outputFileName.ToLower().EndsWith(".json")) outputFileName += ".json";

        JsonHandlingService.SerializeJSONToFIle(obj, outDirectory, outputFileName);
    }
}

