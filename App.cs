
using model;

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
        string input = InputPath();
        RegisterJSON(input);

        Console.WriteLine("\nProcess Ends\nStart again?press 'y' or 'n'");
        ConsoleKey key;
        do
        {
            key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Y)
            {
                new App();
            }
            else
            {
                Console.WriteLine("Good bye");
                break;
            }
        } while (key != ConsoleKey.Y || key != ConsoleKey.N);

    }

    private void RegisterJSON(string path)
    {
        var jsonFiles = Directory.Exists(path)
            ? JsonFileRegister.RegisterFiles(path)
            : new List<JsonFile> { JsonFileRegister.RegisterFile(path) };
    }

    private bool JsonExistsInPath(string path)
    {
        try
        {
            if (Directory.Exists(path) && Path.GetFileName(path).EndsWith("json")) return true;
            var files = Directory.GetFiles(path);

            if (files.Any(file => file.EndsWith(".json"))) return true;

            throw new FileNotFoundException();
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"Directory {path} not found");
            return false;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("JSON File not found");
            return false;
        }
    }
    private string InputPath()
    {
        Console.Write("Input .json file or the path that contains it: ");
        string? path = Console.ReadLine();
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("Input must not null or empty spaces\nTry Again please\n");
            return InputPath();
        }

        if (!JsonExistsInPath(path))
        {
            Console.WriteLine("No JSON file in the path");
            return InputPath();
        }
        return path;
    }
}