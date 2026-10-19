using System.CommandLine;
using Ice;
using MumbleServer;
using Exception = System.Exception;

namespace MumbleServerCli.Action;

public static class TextMessage
{
    private static readonly Argument<string> MessageArg = new("message")
    {
        Description = "Text Message"
    };

    public static Command CreateCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("msg", "Send text messages to channels or users");

        command.Subcommands.Add(CreateWhisperCommand(metaProvider));
        command.Subcommands.Add(CreateBroadcastCommand(metaProvider));
        command.Subcommands.Add(CreateSendMotdCommand(metaProvider));

        return command;
    }

    private static Command CreateWhisperCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("whisper", "Send a text message to a user")
        {
            CommonArgs.IdArg, CommonArgs.SessionArg, MessageArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var sessionId = result.GetValue(CommonArgs.SessionArg);
            var message = result.GetValue(MessageArg);

            server.sendMessage(sessionId, message);
            Console.WriteLine($"Message sent to session {sessionId}.");
        });

        return command;
    }

    private static Command CreateBroadcastCommand(MetaPrxProvider metaProvider)
    {
        var cascadeOpt = new Option<bool>("--cascade")
        {
            Description = "Send the message to all sub-channels",
            DefaultValueFactory = _ => false
        };
        var command = new Command("broadcast", "Send a text message to a channel")
        {
            CommonArgs.IdArg, CommonArgs.ChannelIdArg, MessageArg, cascadeOpt
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var channelId = result.GetValue(CommonArgs.ChannelIdArg);
            var message = result.GetValue(MessageArg);
            var cascade = result.GetValue(cascadeOpt);

            server.sendMessageChannel(channelId, cascade, message);
            Console.WriteLine($"Message sent to channel {channelId}.");
        });

        return command;
    }

    private static Command CreateSendMotdCommand(MetaPrxProvider metaProvider)
    {
        var sessionIdListArg = new Argument<List<int>>("session")
        {
            Description = "Session IDs"
        };
        var command = new Command("motd", "Send the welcome message to users")
        {
            CommonArgs.IdArg, sessionIdListArg
        };
        command.SetAction(result =>
        {
            var server = Utils.ResolveServerFromParseResult(result, metaProvider);
            var sessions = result.GetValue(sessionIdListArg);
            if (sessions is [] or null)
            {
                throw new MumbleServerCliException("You didn't provide any session ID.");
            }

            try
            {
                server.sendWelcomeMessage([.. sessions]);
                Console.WriteLine("Welcome message sent.");
            }
            catch (Exception e) when(e is InvalidSessionException or UnknownUserException { unknown: "::MumbleServer::InvalidSessionException" })
            {
                throw new MumbleServerCliException(
                    "One or more provided sessions not found on virtual server or is invalid.", e);

            }
        });

        return command;
    }
}
