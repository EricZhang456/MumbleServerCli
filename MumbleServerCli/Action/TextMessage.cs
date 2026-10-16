using System.CommandLine;
using MumbleServer;

namespace MumbleServerCli.Action;

public static class TextMessage
{
    private static readonly Argument<string> MessageArg = new("message")
    {
        Description = "Text message to send"
    };

    public static Command CreateCommand(MetaPrxProvider metaProvider)
    {
        var command = new Command("msg", "Send text messages to channels or users");

        command.Subcommands.Add(CreateWhisperCommand(metaProvider));
        command.Subcommands.Add(CreateBroadcastCommand(metaProvider));

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
}
