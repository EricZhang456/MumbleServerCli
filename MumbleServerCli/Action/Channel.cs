using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class Channel
{
    public static Command CreateCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("channel", "Manage channels on a virtual server");

        command.Subcommands.Add(CreateListChannelCommand(metaProvider));
        command.Subcommands.Add(CreateChannelInfoCommand(metaProvider));
        command.Subcommands.Add(CreateAddChannelCommand(metaProvider));
        command.Subcommands.Add(CreateRemoveChannelCommand(metaProvider));
        command.Subcommands.Add(CreateGetDescriptionCommand(metaProvider));

        return command;
    }

    private static Command CreateListChannelCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("list", "List all channels in a virtual server")
        {
            CommonArgs.IdArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);

            try
            {
                var channels = server.getChannels();
                if (channels.Count == 0)
                {
                    Console.WriteLine("There are no channels on this virtual server.");
                    return;
                }

                Console.WriteLine($"Listing {channels.Count} channel(s):\n");
                foreach (var entry in channels)
                {
                    Console.WriteLine($"Channel ID: {entry.Value.id}, Name: \"{entry.Value.name}\"");
                }
            }
            catch (ServerBootedException e)
            {
                throw new MumbleServerCliException("Virtual server is not running.", e);
            }
        });

        return command;
    }

    private static Command CreateChannelInfoCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("info", "Get information about a channel in a virtual server")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var channelId = result.GetValue(CommonArgs.ChannelIdArg);

            try
            {
                var channel = server.getChannelState(channelId);

                Console.WriteLine($"Information about channel {channel.id}:\n");
                Console.WriteLine($"Name: {channel.name}");
                Console.WriteLine($"Description: {channel.description.Truncate()}");
                Console.WriteLine($"Temporary: {Extension.FormatBool(channel.temporary)}");
                Console.WriteLine($"Position: {channel.position}");
                if (channel.parent == -1)
                {
                    Console.WriteLine("Parent: none");
                }
                else
                {
                    try
                    {
                        var parentChannel = server.getChannelState(channel.parent);
                        Console.WriteLine($"Parent: {channel.parent} (Name: \"{parentChannel.name}\")");
                    }
                    catch (InvalidChannelException)
                    {
                        Console.WriteLine($"Parent: {channel.parent}");
                    }
                }

                if (channel.links is not [])
                {
                    Console.WriteLine("Linked Channels:");
                    foreach (var i in channel.links)
                    {
                        try
                        {
                            var linkedChannel = server.getChannelState(i);
                            Console.WriteLine($"\tChannel ID: {linkedChannel.id}, Name: \"{linkedChannel.name}\"");
                        }
                        catch (InvalidChannelException)
                        {
                            Console.WriteLine($"\tChannel ID: {i}");
                        }
                    }
                }
            }
            catch (ServerBootedException e)
            {
                throw new MumbleServerCliException("Virtual server is not running.", e);
            }
            catch (InvalidChannelException e)
            {
                throw new MumbleServerCliException("Channel not found on virtual server.", e);
            }
        });

        return command;
    }

    private static Command CreateAddChannelCommand(MetaPrxProvider metaProvider)
    {
        var channelNameArg = new Argument<string>("name")
        {
            Description = "Channel Name"
        };
        var parentChannelIdOpt = new Option<int>("--parent", "-p")
        {
            Description = "Parent Channel ID",
            DefaultValueFactory = _ => 0
        };
        var command = new Command("create", "Create a new channel in a virtual server")
        {
            CommonArgs.IdArg, channelNameArg, parentChannelIdOpt
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var channelName = result.GetValue(channelNameArg);
            var parentChannelId = result.GetValue(parentChannelIdOpt);

            try
            {
                var newChannelId = server.addChannel(channelName, parentChannelId);
                Console.WriteLine($"Created new channel with ID {newChannelId}.");
            }
            catch (ServerBootedException e)
            {
                throw new MumbleServerCliException("Virtual server is not running.", e);
            }
            catch (InvalidChannelException e)
            {
                throw new MumbleServerCliException("Invalid channel name or parent channel ID.", e);
            }
            catch (NestingLimitException e)
            {
                throw new MumbleServerCliException("Nesting limited reached.", e);
            }
        });

        return command;
    }

    private static Command CreateRemoveChannelCommand(MetaPrxProvider metaProvider)
    {
        // i have no idea how to make rider stop highlighting this as a sql statement so enjoy your blue
        // Delete and from
        var command = new Command("delete", "Delete a channel from a virtual server")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var channelId = result.GetValue(CommonArgs.ChannelIdArg);

            try
            {
                server.removeChannel(channelId);
                Console.WriteLine("Channel deleted.");
            }
            catch (ServerBootedException e)
            {
                throw new MumbleServerCliException("Virtual server is not running.", e);
            }
            catch (InvalidChannelException e)
            {
                throw new MumbleServerCliException(
                    "Channel not found on virtual server or the channel you're trying to delete is the root channel.", e);
            }
        });

        return command;
    }

    private static Command CreateGetDescriptionCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("info", "Get description of a channel in a virtual server")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var channelId = result.GetValue(CommonArgs.ChannelIdArg);

            try
            {
                var channel = server.getChannelState(channelId);

                Console.WriteLine($"Description of channel \"{channel.name}\":\n");
                Console.WriteLine(channel.description);
            }
            catch (ServerBootedException e)
            {
                throw new MumbleServerCliException("Virtual server is not running.", e);
            }
            catch (InvalidChannelException e)
            {
                throw new MumbleServerCliException("Channel not found on virtual server.", e);
            }
        });

        return command;
    }
}
