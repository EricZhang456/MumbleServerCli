using System.Net;
using System.CommandLine;
using MumbleServer;
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
        command.Subcommands.Add(CreateGetCommentCommand(metaProvider));

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

            Console.WriteLine($"Listing {users.Count} user(s):");
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
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var sessionId = result.GetValue(CommonArgs.SessionArg);

            var user = server.getState(sessionId);

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

            Console.WriteLine($"Transfer Rate: {user.bytespersec} bytes/s");
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
}
