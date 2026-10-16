using System.CommandLine;

namespace MumbleServerCli.Action;

public static class Version
{
    public static Command CreateCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("version", "Show version of Mumble master instance");
        command.SetAction(_ =>
        {
            metaProvider.GetMetaPrx().getVersion(out var major, out var minor, out var patch, out var version);
            if (string.IsNullOrWhiteSpace(version))
            {
                version = $"{major}.{minor}.{patch}";
            }

            Console.WriteLine($"Version of Mumble master instance: {version}");
        });

        return command;
    }
}
