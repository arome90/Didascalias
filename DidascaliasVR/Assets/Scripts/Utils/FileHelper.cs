using System.IO;

public class FileHelper
{
    public static string GetTextFromFile(string path)
    {
        StreamReader context = new StreamReader(path);
        return context.ReadToEnd();
    }
}
