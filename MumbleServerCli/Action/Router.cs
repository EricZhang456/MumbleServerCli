using System.CommandLine;

namespace MumbleServerCli.Action;

public static class Router
{
    public static void RegisterActions(RootCommand command, MetaPrxProvider metaProvider)
    {
        command.Subcommands.Add(Server.CreateCommand(metaProvider));
        command.Subcommands.Add(Config.CreateCommand(metaProvider));
        command.Subcommands.Add(Uptime.CreateCommand(metaProvider));
        command.Subcommands.Add(Version.CreateCommand(metaProvider));
        command.Subcommands.Add(User.CreateCommand(metaProvider));
        command.Subcommands.Add(Channel.CreateCommand(metaProvider));
        command.Subcommands.Add(TextMessage.CreateCommand(metaProvider));
    }
}
