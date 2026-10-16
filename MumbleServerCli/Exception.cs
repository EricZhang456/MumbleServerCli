namespace MumbleServerCli;

public class MumbleServerCliException : Exception
{
    public MumbleServerCliException(string message) : base(message)
    {
    }

    public MumbleServerCliException(string message, Exception inner) : base(message, inner)
    {
    }
}
