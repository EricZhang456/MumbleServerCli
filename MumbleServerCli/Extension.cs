namespace MumbleServerCli;

public static class Extension
{
    public static string Truncate(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }

        var lines = str.Split(['\r', '\n']);
        var firstLine = lines[0];

        const int maxLength = 50;

        Console.WriteLine(lines.Length);
        if (firstLine.Length <= maxLength && lines.Length == 1)
        {
            return firstLine;
        }

        var truncatedChars = Math.Min(maxLength, firstLine.Length);
        var truncated = firstLine[..truncatedChars];

        int totalCharactersWithoutNewLines = str.Count(c => c != '\r' && c != '\n');
        int remainingCharacters = totalCharactersWithoutNewLines - truncatedChars;

        return $"{truncated}... <{remainingCharacters} more characters>";
    }
}
