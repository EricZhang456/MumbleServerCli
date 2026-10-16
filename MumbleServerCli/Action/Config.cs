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

    private static readonly Argument<string> OptionalKeyArg = new("key")
    {
        Description = "Configuration key",
        Arity = ArgumentArity.ZeroOrOne
    };

    public static Command CreateCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("config", "Configure virtual servers");

        command.Subcommands.Add(CreateGetCommand(metaProvider));
        command.Subcommands.Add(CreateGetDefaultCommand(metaProvider));
        command.Subcommands.Add(CreateSetCommand(metaProvider));
        command.Subcommands.Add(CreateSetSuperuserCommand(metaProvider));

        return command;
    }

    private static Command CreateGetCommand(MetaPrxProvider metaProvider)
    {
        var getCommand = new Command("get", "Get configuration value(s) for a virtual server")
        {
            CommonArgs.IdArg, OptionalKeyArg
        };
        getCommand.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var id = server.id();
            var key = result.GetValue(OptionalKeyArg);

            if (string.IsNullOrWhiteSpace(key))
            {
                var allConf = server.getAllConf();
                if (allConf.Count == 0)
                {
                    Console.WriteLine($"Virtual server {id} has no configuration values.");
                    return;
                }
                Console.WriteLine($"Configuration values for virtual server {id}:\n");
                foreach (var i in allConf)
                {
                    Console.WriteLine($"{i.Key} = {i.Value.Truncate()}");
                }
                return;
            }

            try
            {
                var value = server.getConf(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine($"Value of \"{key}\" for virtual server {id} is:");
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
        var getDefaultCommand = new Command("getdefault", "Get default configuration value(s) for a virtual server")
        {
            OptionalKeyArg
        };
        getDefaultCommand.SetAction(result =>
        {
            var defaultConf = metaProvider.GetMetaPrx().getDefaultConf();
            var targetKey = result.GetValue(OptionalKeyArg);

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
