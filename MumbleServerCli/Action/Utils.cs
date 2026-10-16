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
        return (server.getState(sessionId), server);
    }
}
