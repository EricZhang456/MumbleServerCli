using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Server
{
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
            CommonArgs.IdArg
        };
        startCommand.SetAction(result =>
        {
            var id = result.GetValue(CommonArgs.IdArg);
            var server = metaProvider.GetMetaPrx().getServer(id);
            if (server is null)
            {
                throw new MumbleServerCliException("Virtual server not found.");
            }

            try
            {
                server.start();
                Console.WriteLine("Virtual server started.");
            }
            catch (ServerBootedException e)
            {
                throw new MumbleServerCliException("Virtual server is already running.", e);
            }
            catch (ServerFailureException e)
            {
                throw new MumbleServerCliException("There was an error when starting the virtual server.", e);
            }
        });

        return startCommand;
    }

    private static Command CreateStopCommand(MetaPrxProvider metaProvider)
    {
        var stopCommand = new Command("stop", "Stop a virtual server")
        {
            CommonArgs.IdArg
        };
        stopCommand.SetAction(result =>
        {
            var id = result.GetValue(CommonArgs.IdArg);
            var server = metaProvider.GetMetaPrx().getServer(id);
            if (server is null)
            {
                throw new MumbleServerCliException("Virtual server not found.");
            }

            try
            {
                server.stop();
                Console.WriteLine("Virtual server stopped.");
            }
            catch (ServerBootedException e)
            {
                throw new MumbleServerCliException("Virtual server is already stopped.", e);
            }
            catch (ServerFailureException e)
            {
                throw new MumbleServerCliException("There was an error when stopping the virtual server.", e);
            }
        });

        return stopCommand;
    }

    private static Command CreateDeleteCommand(MetaPrxProvider metaProvider)
    {
        var deleteCommand = new Command("delete", "Delete a virtual server")
        {
            CommonArgs.IdArg
        };
        deleteCommand.SetAction(result =>
        {
            var id = result.GetValue(CommonArgs.IdArg);
            var server = metaProvider.GetMetaPrx().getServer(id);
            if (server is null)
            {
                throw new MumbleServerCliException("Virtual server not found.");
            }

            try
            {
                server.delete();
                Console.WriteLine("Virtual server deleted.");
            }
            catch (ServerBootedException e)
            {
                throw new MumbleServerCliException("You must stop the virtual server first.", e);
            }
            catch (ServerFailureException e)
            {
                throw new MumbleServerCliException("There was an error when deleting the virtual server.", e);
            }
        });

        return deleteCommand;
    }

    private static Command CreateUptimeCommand(MetaPrxProvider metaProvider)
    {
        var uptimeCommand = new Command("uptime", "Get the uptime of a Mumble virtual server")
        {
            CommonArgs.IdArg
        };
        uptimeCommand.SetAction(result =>
        {
            var id = result.GetValue(CommonArgs.IdArg);
            var server = metaProvider.GetMetaPrx().getServer(id);
            if (server is null)
            {
                throw new MumbleServerCliException("Virtual server not found.");
            }

            try
            {
                var uptime = server.getUptime();
                Console.WriteLine($"Virtual server {id} has been up for: {Extension.FormatTime(uptime)}");
            }
            catch (ServerBootedException e)
            {
                throw new MumbleServerCliException("Virtual server is not running.", e);
            }
        });

        return uptimeCommand;
    }
}
