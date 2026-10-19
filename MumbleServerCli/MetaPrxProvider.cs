using MumbleServer;

namespace MumbleServerCli;

public class MetaPrxProvider
{
    private MetaPrx? _meta = null;

    public void SetMetaPrx(MetaPrx meta)
    {
        _meta = meta;
    }

    public MetaPrx GetMetaPrx() => _meta ?? throw new SystemException("MetaPrx has not been registered yet.");
}
