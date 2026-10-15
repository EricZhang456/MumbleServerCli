using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Server
{
    public static Command CreateServerCommand(MetaPrx meta)
    {
        var command = new Command("server", "Manage virtual servers");

        var idArg = new Argument<int>("id")
        {
            Description = "Virtual server ID"
        };

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

        var startCommand = new Command("start", "Start a virtual server")
        {
            idArg
        };
        startCommand.SetAction(result =>
        {
            var id = result.GetValue(idArg);
            var server = meta.getServer(id);
            if (server == null)
            {
                Console.WriteLine("Virtual server not found.");
                return;
            }

            try
            {
                server.start();
                Console.WriteLine("Virtual server started.");
            }
            catch (ServerBootedException)
            {
                Console.WriteLine("Virtual server is already running.");
            }
            catch (ServerFailureException)
            {
                Console.WriteLine("There was an error when starting the virtual server.");
            }
        });
        command.Subcommands.Add(startCommand);

        var stopCommand = new Command("stop", "Stop a virtual server")
        {
            idArg
        };
        stopCommand.SetAction(result =>
        {
            var id = result.GetValue(idArg);
            var server = meta.getServer(id);
            if (server == null)
            {
                Console.WriteLine("Virtual server not found.");
                return;
            }

            try
            {
                server.stop();
                Console.WriteLine("Virtual server stopped.");
            }
            catch (ServerBootedException)
            {
                Console.WriteLine("Virtual server is already stopped.");
            }
            catch (ServerFailureException)
            {
                Console.WriteLine("There was an error when stopping the virtual server.");
            }
        });
        command.Subcommands.Add(stopCommand);

        var deleteCommand = new Command("delete", "Delete a virtual server")
        {
            idArg
        };
        deleteCommand.SetAction(result =>
        {
            var id = result.GetValue(idArg);
            var server = meta.getServer(id);
            if (server == null)
            {
                Console.WriteLine("Virtual server not found.");
                return;
            }

            try
            {
                server.delete();
                Console.WriteLine("Virtual server deleted.");
            }
            catch (ServerBootedException)
            {
                Console.WriteLine("You must stop the virtual server first.");
            }
            catch (ServerFailureException)
            {
                Console.WriteLine("There was an error when deleting the virtual server.");
            }
        });
        command.Subcommands.Add(deleteCommand);

        return command;
    }
}
