public static class Validation
{

    public static bool FilePathIsValid(string filePath)
    {
        return Directory.Exists(filePath) || filePath.EndsWith(".json");
    }
}