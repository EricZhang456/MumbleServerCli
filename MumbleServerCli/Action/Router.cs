using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Router
{
    public static void RegisterActions(RootCommand command, MetaPrx meta)
    {
        command.Subcommands.Add(Server.CreateServerCommand(meta));
    }
}
