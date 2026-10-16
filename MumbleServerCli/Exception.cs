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

public class MumbleServerNotFoundException : MumbleServerCliException
{
    public MumbleServerNotFoundException() : base("Virtual server not found.")
    {
    }

    public MumbleServerNotFoundException(string message) : base(message)
    {
    }

    public MumbleServerNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}
