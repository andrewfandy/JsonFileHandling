public static class Validation
{

    public static bool IsJsonFile(string filePath)
    {
        return filePath.EndsWith(".json");
    }
}