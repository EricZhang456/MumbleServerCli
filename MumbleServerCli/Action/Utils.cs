using System.CommandLine;

namespace MumbleServerCli.Action;

public static class Utils
{
    public static MumbleServer.ServerPrx ResolveServerFromParseResult(ParseResult result, MetaPrxProvider metaProvider)
    {
        var serverId = result.GetValue(CommonArgs.IdArg);
        var meta = metaProvider.GetMetaPrx();
        var server = meta.getServer(serverId);
        return server ?? throw new MumbleServerNotFoundException();
    }

    public static (MumbleServer.User User, MumbleServer.ServerPrx Server)
        ResolveUserFromParseResult(ParseResult result, MetaPrxProvider metaProvider)
    {
        var server = ResolveServerFromParseResult(result, metaProvider);
        var sessionId = result.GetValue(CommonArgs.SessionArg);
        if (sessionId < 0)
        {
            throw new MumbleServerCliException("Invalid session ID.");
        }
        return (server.getState(sessionId), server);
    }

    public static (MumbleServer.Channel Channel, MumbleServer.ServerPrx Server)
        ResolveChannelFromParseResult(ParseResult result, MetaPrxProvider metaProvider)
    {
        var server = ResolveServerFromParseResult(result, metaProvider);
        var channelId = result.GetValue(CommonArgs.ChannelIdArg);
        if (channelId < 0)
        {
            throw new MumbleServerCliException("Invalid channel ID.");
        }
        return (server.getChannelState(channelId), server);
    }
}
