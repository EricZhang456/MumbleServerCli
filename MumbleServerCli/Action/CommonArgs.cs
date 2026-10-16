using System.CommandLine;

namespace MumbleServerCli.Action;

public static class CommonArgs
{
    public static readonly Argument<int> IdArg = new("id")
    {
        Description = "Virtual Server ID"
    };
}
