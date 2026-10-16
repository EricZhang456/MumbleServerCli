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
}
