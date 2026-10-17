using System.Net;
using System.CommandLine;
using MumbleServerCli.Version;

namespace MumbleServerCli.Action;

public static class User
{
    public static Command CreateCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("user", "Manage users on a virtual server");

        command.Subcommands.Add(CreateListUsersCommand(metaProvider));
        command.Subcommands.Add(CreateUserInfoCommand(metaProvider));
        command.Subcommands.Add(CreateKickUserCommand(metaProvider));
        command.Subcommands.Add(CreateMuteUserCommand(metaProvider));
        command.Subcommands.Add(CreateDeafenUserCommand(metaProvider));
        command.Subcommands.Add(CreateSuppressUserCommand(metaProvider));
        command.Subcommands.Add(CreatePrioritySpeakerCommand(metaProvider));
        command.Subcommands.Add(CreateMoveCommand(metaProvider));
        command.Subcommands.Add(CreateRenameCommand(metaProvider));
        command.Subcommands.Add(CreateGetCommentCommand(metaProvider));
        command.Subcommands.Add(CreateSetCommentCommand(metaProvider));

        return command;
    }

    private static Command CreateListUsersCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("list", "List all users on a virtual server")
        {
            CommonArgs.IdArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);

            var users = server.getUsers();
            if (users.Count == 0)
            {
                Console.WriteLine("There are no users on this virtual server.");
                return;
            }

            Console.WriteLine($"Listing {users.Count} user(s):\n");
            Console.WriteLine();
            foreach (var entry in users)
            {
                Console.WriteLine($"Session ID: {entry.Value.session}, Name: \"{entry.Value.name}\"");
            }
        });

        return command;
    }

    private static Command CreateUserInfoCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("info", "Get information about a user on a virtual server")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg
        };
        command.SetAction(result =>
        {
            var user = Utils.ResolveUserFromParseResult(result, metaProvider).User;

            Console.WriteLine($"Information about session {user.session}:\n");
            Console.WriteLine($"Name: {user.name}");
            Console.WriteLine($"User ID: {(user.userid == -1 ? "anonymous" : user.userid)}");
            Console.WriteLine($"Time: {Extension.FormatTime(user.onlinesecs)}");
            if (user.idlesecs > 0)
            {
                Console.WriteLine($"Idle Time: {Extension.FormatTime(user.idlesecs)}");
            }

            if (!string.IsNullOrWhiteSpace(user.comment))
            {
                Console.WriteLine($"Comment: {user.comment.Truncate()}");
            }

            if (user.bytespersec > 0)
            {
                Console.WriteLine($"Transfer Rate: {user.bytespersec} bytes/s");
            }

            Console.WriteLine($"TCP Only: {Extension.FormatBool(user.tcponly)}");
            Console.WriteLine($"TCP Ping: {Math.Round(user.tcpPing, 2)}");
            if (!user.tcponly)
            {
                Console.WriteLine($"UDP Ping: {Math.Round(user.udpPing, 2)}");
            }

            if (user.version2 > 0)
            {
                var version = new MumbleVersion(user.version2);
                Console.WriteLine($"Version: {version}");
            }
            else
            {
                var version = new LegacyMumbleVersion(user.version);
                Console.WriteLine($"Version: {version}");
            }

            Console.WriteLine($"OS: {user.os}");
            Console.WriteLine($"OS Version: {user.osversion}");
            try
            {
                var ipAddr = new IPAddress(user.address);
                if (ipAddr.IsIPv4MappedToIPv6)
                {
                    ipAddr = ipAddr.MapToIPv4();
                }

                Console.WriteLine($"IP Address: {ipAddr}");
            }
            catch (ArgumentException)
            {
            }

            Console.WriteLine($"Self Muted: {Extension.FormatBool(user.selfMute)}");
            Console.WriteLine($"Self Deafened: {Extension.FormatBool(user.selfDeaf)}");
            Console.WriteLine($"Muted by Server: {Extension.FormatBool(user.mute)}");
            Console.WriteLine($"Deafened by Server: {Extension.FormatBool(user.deaf)}");
            Console.WriteLine($"Suppressed: {Extension.FormatBool(user.suppress)}");
            Console.WriteLine($"Priority Speaker: {Extension.FormatBool(user.prioritySpeaker)}");
            Console.WriteLine($"Recording: {Extension.FormatBool(user.recording)}");
            Console.WriteLine($"Channel ID: {user.channel}");
            if (!string.IsNullOrWhiteSpace(user.identity))
            {
                Console.WriteLine($"Plugin Identity: {user.identity}");
            }
        });

        return command;
    }

    private static Command CreateKickUserCommand(MetaPrxProvider metaProvider)
    {
        var reasonArg = new Argument<string>("reason")
        {
            Description = "Reason for kicking the user",
            Arity = ArgumentArity.ZeroOrOne
        };
        var command = new Command("kick", "Kick a user from a virtual server")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg, reasonArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var sessionId = result.GetValue(CommonArgs.SessionArg);
            var reason = result.GetValue(reasonArg);

            server.kickUser(sessionId, string.IsNullOrWhiteSpace(reason) ? string.Empty : reason);
            Console.WriteLine("User kicked.");
        });

        return command;
    }

    private static Command CreateMuteUserCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("mute", "Mute a user")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg
        };
        command.SetAction(result =>
        {
            var (user, server) = Utils.ResolveUserFromParseResult(result, metaProvider);

            if (user.mute)
            {
                throw new MumbleServerCliException("User is already muted by the server.");
            }

            user.mute = true;
            server.setState(user);
        });

        return command;
    }

    private static Command CreateDeafenUserCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("deafen", "Deafen a user")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg
        };
        command.SetAction(result =>
        {
            var (user, server) = Utils.ResolveUserFromParseResult(result, metaProvider);

            if (user.deaf)
            {
                throw new MumbleServerCliException("User is already deafened by the server.");
            }

            user.deaf = true;
            server.setState(user);
        });

        return command;
    }

    private static Command CreateSuppressUserCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("suppress", "Suppress a user")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg
        };
        command.SetAction(result =>
        {
            var (user, server) = Utils.ResolveUserFromParseResult(result, metaProvider);

            if (user.suppress)
            {
                throw new MumbleServerCliException("User is already suppressed.");
            }

            user.suppress = true;
            server.setState(user);
        });

        return command;
    }

    private static Command CreatePrioritySpeakerCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("priority", "Make user a priority speaker")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg
        };
        command.SetAction(result =>
        {
            var (user, server) = Utils.ResolveUserFromParseResult(result, metaProvider);

            if (user.prioritySpeaker)
            {
                throw new MumbleServerCliException("User is already a priority Speaker.");
            }

            user.prioritySpeaker = true;
            server.setState(user);
        });

        return command;
    }

    private static Command CreateMoveCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("move", "Move a user to a different channel")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg, CommonArgs.ChannelIdArg
        };
        command.SetAction(result =>
        {
            var (user, server) = Utils.ResolveUserFromParseResult(result, metaProvider);
            var channelId = result.GetValue(CommonArgs.ChannelIdArg);

            if (user.channel == channelId)
            {
                throw new MumbleServerCliException($"User is already in channel {channelId}.");
            }

            user.channel = channelId;
            server.setState(user);
        });

        return command;
    }

    private static Command CreateRenameCommand(MetaPrxProvider metaProvider)
    {
        var nameArg = new Argument<string>("name")
        {
            Description = "Username"
        };
        var command = new Command("rename", "Rename a user")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg, nameArg
        };
        command.SetAction(result =>
        {
            var name = result.GetValue(nameArg)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new MumbleServerCliException("Provided username is empty");
            }

            var (user, server) = Utils.ResolveUserFromParseResult(result, metaProvider);

            if (user.name == name)
            {
                throw new MumbleServerCliException("Provided username is the same as the user's current username.");
            }

            user.name = name;
            server.setState(user);
        });

        return command;
    }

    private static Command CreateGetCommentCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("getcomment", "Get comment of a user on a virtual server")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var sessionId = result.GetValue(CommonArgs.SessionArg);

            var user = server.getState(sessionId);

            if (string.IsNullOrWhiteSpace(user.comment))
            {
                Console.WriteLine($"User \"{user.name}\" has no comment set.");
                return;
            }

            Console.WriteLine($"Comment for user \"{user.name}\":\n");
            Console.WriteLine(user.comment);
        });

       return command;
    }

    private static Command CreateSetCommentCommand(MetaPrxProvider metaProvider)
    {
        var commentArg = new Argument<string>("comment")
        {
            Description = "User Comment"
        };
        var command = new Command("setcomment", "Set comment on a user")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg, commentArg
        };
        command.SetAction(result =>
        {
            var comment = result.GetValue(commentArg)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new MumbleServerCliException("Provided user comment is empty");
            }

            var (user, server) = Utils.ResolveUserFromParseResult(result, metaProvider);

            if (user.comment == comment)
            {
                throw new MumbleServerCliException("Provided user comment is the same as the user's current comment.");
            }

            user.comment = comment;
            server.setState(user);
        });

        return command;
    }
}
