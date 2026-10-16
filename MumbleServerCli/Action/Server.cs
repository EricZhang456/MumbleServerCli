using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Server
{
    private static readonly Argument<int> IdArg = new("id")
    {
        Description = "Virtual server ID"
    };

    public static Command CreateServerCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("server", "Manage virtual servers");

        command.Subcommands.Add(CreateListCommand(metaProvider));
        command.Subcommands.Add(CreateAddCommand(metaProvider));
        command.Subcommands.Add(CreateStartCommand(metaProvider));
        command.Subcommands.Add(CreateStopCommand(metaProvider));
        command.Subcommands.Add(CreateDeleteCommand(metaProvider));
        command.Subcommands.Add(CreateUptimeCommand(metaProvider));

        return command;
    }

    private static Command CreateListCommand(MetaPrxProvider metaProvider)
    {
        var listCommand = new Command("list", "List all virtual servers");
        listCommand.SetAction(_ =>
        {
            var servers = metaProvider.GetMetaPrx().getAllServers();
            if (servers is null or [])
            {
                Console.WriteLine("There are no virtual servers.");
                return;
            }
            Console.WriteLine($"Listing {servers.Length} server(s):");
            foreach (var instance in servers)
            {
                var status = instance.isRunning() ? "running" : "stopped";
                Console.WriteLine($"ID: {instance.id()}, Status: {status}");
            }
        });

        return listCommand;
    }

    private static Command CreateAddCommand(MetaPrxProvider metaProvider)
    {
        var addCommand = new Command("add", "Add a new virtual server");
        addCommand.SetAction(_ =>
        {
            var server = metaProvider.GetMetaPrx().newServer();
            Console.WriteLine($"Created virtual server with ID {server.id()}");
        });

        return addCommand;
    }

    private static Command CreateStartCommand(MetaPrxProvider metaProvider)
    {
        var startCommand = new Command("start", "Start a virtual server")
        {
            IdArg
        };
        startCommand.SetAction(result =>
        {
            var id = result.GetValue(IdArg);
            var server = metaProvider.GetMetaPrx().getServer(id);
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

        return startCommand;
    }

    private static Command CreateStopCommand(MetaPrxProvider metaProvider)
    {
        var stopCommand = new Command("stop", "Stop a virtual server")
        {
            IdArg
        };
        stopCommand.SetAction(result =>
        {
            var id = result.GetValue(IdArg);
            var server = metaProvider.GetMetaPrx().getServer(id);
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

        return stopCommand;
    }

    private static Command CreateDeleteCommand(MetaPrxProvider metaProvider)
    {
        var deleteCommand = new Command("delete", "Delete a virtual server")
        {
            IdArg
        };
        deleteCommand.SetAction(result =>
        {
            var id = result.GetValue(IdArg);
            var server = metaProvider.GetMetaPrx().getServer(id);
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

        return deleteCommand;
    }

    private static Command CreateUptimeCommand(MetaPrxProvider metaProvider)
    {
        var uptimeCommand = new Command("uptime", "Get the uptime of a Mumble virtual server")
        {
            IdArg
        };
        uptimeCommand.SetAction(result =>
        {
            var id = result.GetValue(IdArg);
            var server = metaProvider.GetMetaPrx().getServer(id);
            if (server == null)
            {
                Console.WriteLine("Virtual server not found.");
                return;
            }

            try
            {
                var uptime = server.getUptime();
                Console.WriteLine($"Virtual server {id} has been up for: {Extension.FormatTime(uptime)}");
            }
            catch (ServerBootedException)
            {
                Console.WriteLine("Server is not running");
            }
        });

        return uptimeCommand;
    }
}
