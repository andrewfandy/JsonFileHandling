
using model;
using services;
using utils;

namespace JsonFileHandling;


public class App
{
    public App()
    {
        Console.WriteLine("=== JSON FILE HANDLER APP ===");
        Run();
    }


    private void Run()
    {
        Console.WriteLine("INPUT DRAFT FILE");
        var draftFile = JsonFileRegister.RegisterFile(InputHelper.Input("Input .json file: "));

        Console.WriteLine("\nINPUT CONFIRMED FILE");
        var confirmedFile = JsonFileRegister.RegisterFile(InputHelper.Input("Input .json file: "));

        var compared = JsonHandlingService.CompareAndMerge(draftFile, confirmedFile);

        if (compared == null)
        {
            Console.WriteLine("Compared JObject null");
            return;
        }
        var matchedDb = JsonHandlingService.CompareFieldsOnDB(compared);


        string outDirectory = InputHelper.Input("INPUT OUTPUT DIR: ");
        string outputFileName = InputHelper.Input("INPUT OUTPUT FILE NAME: ");
        if (!outputFileName.ToLower().EndsWith(".json")) outputFileName += ".json";

        JsonHandlingService.SerializeJSON(matchedDb, outDirectory, outputFileName);

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
}

