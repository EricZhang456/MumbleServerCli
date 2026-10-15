using System.Text;

namespace MumbleServerCli;

public static class Extension
{
    public static string Truncate(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }

        var lines = str.Split('\r', '\n');
        var firstLine = lines[0];

        const int maxLength = 50;

        Console.WriteLine(lines.Length);
        if (firstLine.Length <= maxLength && lines.Length == 1)
        {
            return firstLine;
        }

        var truncatedChars = Math.Min(maxLength, firstLine.Length);
        var truncated = firstLine[..truncatedChars];

        var totalCharactersWithoutNewLines = str.Count(c => c != '\r' && c != '\n');
        var remainingCharacters = totalCharactersWithoutNewLines - truncatedChars;

        return $"{truncated}... <{remainingCharacters} more characters>";
    }

    public static string FormatTime(int seconds)
    {
        var timespan = TimeSpan.FromSeconds(seconds);

        var builder = new StringBuilder();

        builder.Append($"{timespan.Seconds} seconds");

        if (timespan.Minutes >= 1)
        {
            builder.Insert(0, $"{timespan.Minutes} minutes, ");
        }

        if (timespan.Hours >= 1)
        {
            builder.Insert(0, $"{timespan.Hours} hours, ");
        }

        if (timespan.Days >= 1)
        {
            builder.Insert(0, $"{timespan.Days} days, ");
        }

        return builder.ToString();
    }
}
