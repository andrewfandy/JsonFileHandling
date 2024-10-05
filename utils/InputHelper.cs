namespace utils;

public static class InputHelper
{
    public static string Input(string message)
    {
        Console.Write(message);
        string? input = Console.ReadLine();
        if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Input must not null or empty spaces\nTry Again please\n");
            return Input(message);
        }

        return input;
    }
    public static string Input()
    {
        return Input("Input: ");
    }

    public static int InputToInt(string message)
    {
        if (int.TryParse(Input(message), out int result))
        {
            return result;
        }
        else
        {
            return InputToInt(message);
        }
    }
    public static int InputToInt()
    {
        return InputToInt("Input: ");
    }
}
