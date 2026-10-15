using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Server
{
    public static Command CreateServerCommand(MetaPrx meta)
    {
        var command = new Command("server", "Manage virtual servers.");

        var getCommand = new Command("get", "Get all virtual servers");
        getCommand.SetAction(_ =>
        {
            var servers = meta.getAllServers();
            if (servers is null or [])
            {
                Console.WriteLine("There are no virtual servers.");
                return;
            }
            Console.WriteLine($"Listing {servers.Length} servers:");
            foreach (var instance in servers)
            {
                Console.WriteLine($"ID: {instance.id()}, Status: {(instance.isRunning() ? "running" : "stopped")}");
            }
        });
        command.Subcommands.Add(getCommand);

        var addCommand = new Command("add", "Add a new virtual server");
        addCommand.SetAction(_ =>
        {
            var server = meta.newServer();
            Console.WriteLine($"Created virtual server with ID {server.id()}");
        });
        command.Subcommands.Add(addCommand);

        return command;
    }
}
