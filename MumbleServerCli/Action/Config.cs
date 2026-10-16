using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Config
{
    private static readonly Argument<string> KeyArg = new("key")
    {
        Description = "Configuration key"
    };

    private static readonly Argument<string> ValueArg = new("value")
    {
        Description = "Configuration value"
    };

    private static readonly Argument<string> SuperuserPasswordArg = new("password")
    {
        Description = "Superuser password"
    };

    public static Command CreateCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("config", "Configure virtual servers");

        command.Subcommands.Add(CreateGetAllCommand(metaProvider));
        command.Subcommands.Add(CreateGetCommand(metaProvider));
        command.Subcommands.Add(CreateGetDefaultCommand(metaProvider));
        command.Subcommands.Add(CreateSetCommand(metaProvider));
        command.Subcommands.Add(CreateSetSuperuserCommand(metaProvider));

        return command;
    }

    private static Command CreateGetAllCommand(MetaPrxProvider metaProvider)
    {
        var getAllCommand = new Command("getall", "Get all configuration values for a virtual server")
        {
            CommonArgs.IdArg
        };
        getAllCommand.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);

            var allConf = server.getAllConf();
            Console.WriteLine($"Configuration values for virtual server ID {server.id()}:\n");
            foreach (var i in allConf)
            {
                Console.WriteLine($"{i.Key} = {i.Value.Truncate()}");
            }
        });

        return getAllCommand;
    }

    private static Command CreateGetCommand(MetaPrxProvider metaProvider)
    {
        var getCommand = new Command("get", "Get configuration value for a virtual server")
        {
            CommonArgs.IdArg, KeyArg
        };
        getCommand.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var id = server.id();
            var key = result.GetValue(KeyArg);

            try
            {
                var value = server.getConf(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine($"Value of \"{key}\" for virtual server ID {id} is:");
                    Console.WriteLine($"\n{value}");
                }
                else
                {
                    Console.WriteLine($"\"{key}\" is unset for virtual server {id}.");
                }
            }
            catch (WriteOnlyException)
            {
                Console.WriteLine("This value is secret.");
            }
        });

        return getCommand;
    }

    private static Command CreateGetDefaultCommand(MetaPrxProvider metaProvider)
    {
        var optionalKeyArg = new Argument<string>("key")
        {
            Description = "Configuration key",
            Arity = ArgumentArity.ZeroOrOne
        };
        var getDefaultCommand = new Command("getdefault", "Get default configuration values for a virtual server")
        {
            optionalKeyArg
        };
        getDefaultCommand.SetAction(result =>
        {
            var defaultConf = metaProvider.GetMetaPrx().getDefaultConf();
            var targetKey = result.GetValue(optionalKeyArg);

            if (string.IsNullOrWhiteSpace(targetKey))
            {
                Console.WriteLine("Default configuration for all virtual servers:\n");
                foreach (var i in defaultConf)
                {
                    Console.WriteLine($"{i.Key} = {i.Value.Truncate()}");
                }
            }
            else
            {
                if (defaultConf.TryGetValue(targetKey, out var value))
                {
                    Console.WriteLine($"Value of \"{targetKey}\" for default configuration is:");
                    Console.WriteLine($"\n{value}");
                }
                else
                {
                    Console.WriteLine($"\"{targetKey}\" is unset in default configuration.");
                }
            }
        });

        return getDefaultCommand;
    }

    private static Command CreateSetCommand(MetaPrxProvider metaProvider)
    {
        var setCommand = new Command("set", "Set configuration value for a virtual server")
        {
            CommonArgs.IdArg, KeyArg, ValueArg
        };
        setCommand.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var key = result.GetValue(KeyArg);
            var value = result.GetValue(ValueArg);

            server.setConf(key, value);
            Console.WriteLine("Configuration value changed");
        });

        return setCommand;
    }

    private static Command CreateSetSuperuserCommand(MetaPrxProvider metaProvider)
    {
        var setSuperuserPasswordCommand = new Command("setsupw", "Set Superuser password for a virtual server")
        {
            CommonArgs.IdArg, SuperuserPasswordArg
        };
        setSuperuserPasswordCommand.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var password = result.GetValue(SuperuserPasswordArg);

            server.setSuperuserPassword(password);
            Console.WriteLine("Superuser password changed");
        });

        return setSuperuserPasswordCommand;
    }
}
