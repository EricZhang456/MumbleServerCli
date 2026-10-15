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

        if (firstLine.Length <= maxLength && lines.Length == 1)
        {
            return firstLine;
        }

        var truncatedChars = Math.Min(maxLength, firstLine.Length);
        var truncated = firstLine[..truncatedChars];

        var remainingCharacters = str.Length - truncatedChars;

        return $"{truncated}... <{remainingCharacters} more characters>";
    }

    public static string FormatTime(int seconds)
    {
        var timespan = TimeSpan.FromSeconds(seconds);

        List<string> strings = [];

        if (timespan.Seconds >= 1)
        {
            var secondsStr = timespan.Seconds == 1 ? "second" : "seconds";
            strings.Insert(0, $"{timespan.Seconds} {secondsStr}");
        }

        if (timespan.Minutes >= 1)
        {
            var minuteStr = timespan.Minutes == 1 ? "minute" : "minutes";
            strings.Insert(0, $"{timespan.Minutes} {minuteStr}");
        }

        if (timespan.Hours >= 1)
        {
            var hourStr = timespan.Hours == 1 ? "hour" : "hours";
            strings.Insert(0, $"{timespan.Hours} {hourStr}");
        }

        if (timespan.Days >= 1)
        {
            var dayStr = timespan.Days == 1 ? "day" : "days";
            strings.Insert(0, $"{timespan.Days} {dayStr}");
        }

        return string.Join(", ", strings);
    }
}
