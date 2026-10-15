using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Router
{
    public static void RegisterActions(RootCommand command, MetaPrxProvider metaProvider)
    {
        command.Subcommands.Add(Server.CreateServerCommand(metaProvider));
        command.Subcommands.Add(Config.CreateConfigCommand(metaProvider));
        command.Subcommands.Add(Uptime.CreateUptimeCommand(metaProvider));
    }
}
