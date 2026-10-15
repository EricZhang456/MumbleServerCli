using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Uptime
{
    public static Command CreateUptimeCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("uptime", "Get the uptime of Mumble master instance");
        command.SetAction(_ =>
        {
            var uptime = metaProvider.GetMetaPrx().getUptime();
            Console.WriteLine($"The master instance has been up for: {Extension.FormatTime(uptime)}");
        });

        return command;
    }
}
