using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Config
{
    public static Command CreateConfigCommand(MetaPrx meta)
    {
        var command = new Command("config", "Configure virtual servers");

        var idArg = new Argument<int>("id")
        {
            Description = "Virtual Server ID"
        };

        var keyArg = new Argument<string>("key")
        {
            Description = "Configuration key"
        };

        var valueArg = new Argument<string>("value")
        {
            Description = "Configuration value"
        };

        var superuserPasswordArg = new Argument<string>("password")
        {
            Description = "Superuser password"
        };

        var getAllCommand = new Command("getall", "Get all configuration values for a virtual server")
        {
            idArg
        };
        getAllCommand.SetAction(result =>
        {
            var id = result.GetValue(idArg);

            var server = meta.getServer(id);
            if (server == null)
            {
                Console.WriteLine("Virtual server not found.");
                return;
            }

            var allConf = server.getAllConf();
            Console.WriteLine($"Configuration values for virtual server ID {id}:\n");
            foreach (var i in allConf)
            {
                Console.WriteLine($"{i.Key} = {i.Value.Truncate()}");
            }
        });
        command.Subcommands.Add(getAllCommand);

        var getCommand = new Command("get", "Get configuration value for a virtual server")
        {
            idArg, keyArg
        };
        getCommand.SetAction(result =>
        {
            var id = result.GetValue(idArg);
            var key = result.GetValue(keyArg);

            var server = meta.getServer(id);
            if (server == null)
            {
                Console.WriteLine("Virtual server not found.");
                return;
            }

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
        command.Subcommands.Add(getCommand);

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
            var defaultConf = meta.getDefaultConf();
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
        command.Subcommands.Add(getDefaultCommand);

        var setCommand = new Command("set", "Set configuration value for a virtual server")
        {
            idArg, keyArg, valueArg
        };
        setCommand.SetAction(result =>
        {
            var id = result.GetValue(idArg);
            var key = result.GetValue(keyArg);
            var value = result.GetValue(valueArg);

            var server = meta.getServer(id);
            if (server == null)
            {
                Console.WriteLine("Virtual server not found.");
                return;
            }

            server.setConf(key, value);
            Console.WriteLine("Configuration value changed");
        });
        command.Subcommands.Add(setCommand);

        var setSuperuserPasswordCommand = new Command("setsupw", "Set Superuser password for a virtual server")
        {
            idArg, superuserPasswordArg
        };
        setSuperuserPasswordCommand.SetAction(result =>
        {
            var id = result.GetValue(idArg);
            var password = result.GetValue(superuserPasswordArg);

            var server = meta.getServer(id);
            if (server == null)
            {
                Console.WriteLine("Virtual server not found.");
                return;
            }

            server.setSuperuserPassword(password);
            Console.WriteLine("Superuser password changed");
        });
        command.Subcommands.Add(setSuperuserPasswordCommand);

        return command;
    }
}
