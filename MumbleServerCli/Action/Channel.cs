using System.CommandLine;
using IceGrid;
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
        command.Subcommands.Add(CreateRenameChannelCommand(metaProvider));
        command.Subcommands.Add(CreateSetParentCommand(metaProvider));
        command.Subcommands.Add(CreateSetPositionCommand(metaProvider));
        command.Subcommands.Add(CreateGetListenersCommand(metaProvider));
        command.Subcommands.Add(CreateGetDescriptionCommand(metaProvider));
        command.Subcommands.Add(CreateSetDescriptionCommand(metaProvider));

        return command;
    }

    private static Command CreateListChannelCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("list", "List all channels")
        {
            CommonArgs.IdArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);

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
        });

        return command;
    }

    private static Command CreateChannelInfoCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("info", "Get information about a channel")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg
        };
        command.SetAction(result =>
        {
            var (channel, server) = Utils.ResolveChannelFromParseResult(result, metaProvider);

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

            if (channel.links is [] or null)
            {
                return;
            }

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
        var command = new Command("create", "Create a new channel")
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
            catch (InvalidChannelException e)
            {
                throw new MumbleServerCliException(
                    "Channel not found on virtual server or the channel you're trying to delete is the root channel.", e);
            }
        });

        return command;
    }

    private static Command CreateRenameChannelCommand(MetaPrxProvider metaProvider)
    {
        var nameArg = new Argument<string>("name")
        {
            Description = "Channel Name"
        };
        var command = new Command("rename", "Rename a channel")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg, nameArg
        };
        command.SetAction(result =>
        {
            var name = result.GetValue(nameArg)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new MumbleServerCliException("Provided channel name is empty.");
            }

            var (channel, server) = Utils.ResolveChannelFromParseResult(result, metaProvider);
            if (channel.name == name)
            {
                throw new MumbleServerCliException("Provided channel name is the same as the channel's current name.");
            }

            channel.name = name;
            server.setChannelState(channel);

            Console.WriteLine($"Channel renamed to \"{name}\".");
        });

        return command;
    }

    private static Command CreateSetParentCommand(MetaPrxProvider metaProvider)
    {
        var parentArg = new Argument<int>("parent")
        {
            Description = "Parent Channel"
        };
        var command = new Command("setparent", "Set parent of a channel")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg, parentArg
        };
        command.SetAction(result =>
        {
            var parent = result.GetValue(parentArg);
            if (parent < 0)
            {
                throw new MumbleServerCliException("Invalid parent channel ID.");
            }

            var (channel, server) = Utils.ResolveChannelFromParseResult(result, metaProvider);
            if (channel.parent == parent)
            {
                throw new MumbleServerCliException(
                    "Provided parent channel is the same as the channel's current parent channel");
            }

            channel.parent = parent;
            try
            {
                server.setChannelState(channel);
                Console.WriteLine($"Channel parent changed to {parent}.");
            }
            catch (NestingLimitException e)
            {
                throw new MumbleServerCliException("Nesting limit reached.", e);
            }
            catch (InvalidChannelException e)
            {
                throw new MumbleServerCliException("Provided parent channel not found on virtual server.", e);
            }
        });

        return command;
    }

    private static Command CreateSetPositionCommand(MetaPrxProvider metaProvider)
    {
        var posArg = new Argument<int>("position")
        {
            Description = "Position"
        };
        var command = new Command("setpos", "Set position of a channel")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg, posArg
        };
        command.SetAction(result =>
        {
            var position = result.GetValue(posArg);
            if (position < 0)
            {
                throw new MumbleServerCliException("Invalid position.");
            }

            var (channel, server) = Utils.ResolveChannelFromParseResult(result, metaProvider);
            if (channel.position == position)
            {
                throw new MumbleServerCliException("Provided position is the same as the channel's current position.");
            }

            channel.position = position;
            server.setChannelState(channel);

            Console.WriteLine("Channel position changed.");
        });

        return command;
    }

    private static Command CreateGetListenersCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("getlisteners", "Get a list of listeners in a channel")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var channelId = result.GetValue(CommonArgs.ChannelIdArg);

            var listeners = server.getListeningUsers(channelId);
            if (listeners is [] or null)
            {
                Console.WriteLine($"Channel {channelId} has no listeners.");
                return;
            }

            Console.WriteLine($"Listener(s) in channel {channelId}:\n");
            foreach (var i in listeners)
            {
                try
                {
                    var user = server.getState(i);
                    Console.WriteLine($"Session ID: {user.session}, Name: \"{user.name}\"");
                }
                catch (InvalidSessionException)
                {
                    Console.WriteLine($"Session ID: {i}");
                }
            }
        });

        return command;
    }

    private static Command CreateGetDescriptionCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("getdesc", "Get description of a channel")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg
        };
        command.SetAction(result =>
        {
            var channel = Utils.ResolveChannelFromParseResult(result, metaProvider).Channel;

            if (string.IsNullOrWhiteSpace(channel.description))
            {
                Console.WriteLine($"Channel \"{channel.name}\" has no description set.");
                return;
            }

            Console.WriteLine($"Description of channel \"{channel.name}\":\n");
            Console.WriteLine(channel.description);
        });

        return command;
    }

    private static Command CreateSetDescriptionCommand(MetaPrxProvider metaProvider)
    {
        var descArg = new Argument<string>("description")
        {
            Description = "Description"
        };
        var command = new Command("setdesc", "Set description of a channel")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg, descArg
        };
        command.SetAction(result =>
        {
            var desc = result.GetValue(descArg)?.Trim() ?? string.Empty;
            var (channel, server) = Utils.ResolveChannelFromParseResult(result, metaProvider);

            if (channel.description == desc)
            {
                throw new MumbleServerCliException(
                    "Provided description is the same as the channel's current description.");
            }

            channel.description = desc;
            server.setChannelState(channel);

            Console.WriteLine(!string.IsNullOrWhiteSpace(desc) ?
                "Channel description changed." : "Channel description removed.");
        });

        return command;
    }
}
