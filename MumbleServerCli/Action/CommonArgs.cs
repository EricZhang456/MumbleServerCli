using System.CommandLine;

namespace MumbleServerCli.Action;

public static class CommonArgs
{
    public static readonly Argument<int> IdArg = new("id")
    {
        Description = "Virtual Server ID"
    };

    public static readonly Argument<int> ChannelIdArg = new("channel")
    {
        Description = "Channel ID"
    };

    public static readonly Argument<int> SessionArg = new("session")
    {
        Description = "User Session ID"
    };
}
