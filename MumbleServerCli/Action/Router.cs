using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Router
{
    public static void RegisterActions(RootCommand command, MetaPrx meta)
    {
        command.Subcommands.Add(Server.CreateServerCommand(meta));
        command.Subcommands.Add(Config.CreateConfigCommand(meta));
        command.Subcommands.Add(Uptime.CreateUptimeCommand(meta));
    }
}
