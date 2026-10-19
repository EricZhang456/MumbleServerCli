using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Ice;
using MumbleServer;
using MumbleServerCli;

using Exception = System.Exception;
using Router = MumbleServerCli.Action.Router;
using TimeoutException = Ice.TimeoutException;

const string CONN_STR_ENVVAR = "MUMBLE_SERVER_CLI_CONN_STR";
const string SECRET_ENV_VAR = "MUMBLE_SERVER_CLI_SECRET";

var rootCommand = new RootCommand("Control mumble-server via the Ice interface.");

var metaProvider = new MetaPrxProvider();
Router.RegisterActions(rootCommand, metaProvider);

var parseResult = rootCommand.Parse(args);
// invoke now if we have errors so we can print them
if (parseResult.Action is ParseErrorAction)
{
    parseResult.Invoke();
    return 1;
}

// VersionOptionAction is private so this is what we'll need to do
var versionActionType = new VersionOption().Action!.GetType();
// invoke now if --help or --version is issued
if (parseResult.Action is HelpAction || parseResult.Action?.GetType() == versionActionType)
{
    parseResult.Invoke();
    return 0;
}

var connStr = Environment.GetEnvironmentVariable(CONN_STR_ENVVAR);
var secret = Environment.GetEnvironmentVariable(SECRET_ENV_VAR);
var warned = false;
var secretSet = !string.IsNullOrWhiteSpace(secret);

if (string.IsNullOrWhiteSpace(connStr))
{
    Console.WriteLine($"Warning: {CONN_STR_ENVVAR} not set. Defaulting to \"tcp -h 127.0.0.1 -p 6502 -t 60000\"");
    connStr = "tcp -h 127.0.0.1 -p 6502 -t 60000";
    warned = true;
}

if (!secretSet)
{
    Console.WriteLine($"Warning: {SECRET_ENV_VAR} not set.");
    warned = true;
}

if (warned)
{
    Console.WriteLine();
}

try
{
    var props = Util.createProperties();
    props.setProperty("Ice.ImplicitContext", "Shared");

    var initData = new InitializationData
    {
        properties = props
    };

    using var communicator = Ice.Util.initialize(initData);
    if (secretSet)
    {
        communicator.getImplicitContext().put("secret", secret);
    }

    var obj = communicator.stringToProxy("Meta:" + connStr);
    var meta = MetaPrxHelper.checkedCast(obj);
    if (meta is null)
    {
        Console.WriteLine("Provided Ice server is not a Mumble server");
        return 1;
    }

    metaProvider.SetMetaPrx(meta);

    var invokeConfig = new InvocationConfiguration
    {
        EnableDefaultExceptionHandler = false
    };
    parseResult.Invoke(invokeConfig);

    return 0;
}
catch (ConnectionRefusedException)
{
    Console.WriteLine("Connection refused.");
    return 1;
}
catch (TimeoutException)
{
    Console.WriteLine("Connection timed out.");
    return 1;
}
catch (Exception e) when (e is InvalidSecretException || (e is UnknownUserException ex && ex.unknown == "::MumbleServer::InvalidSessionException"))
{
    Console.WriteLine($"Invalid secret. Set {SECRET_ENV_VAR} to the correct secret.");
    return 1;
}
catch (Exception e) when (e is ServerBootedException || (e is UnknownUserException ex && ex.unknown == "::MumbleServer::ServerBootedException"))
{
    Console.WriteLine("Virtual server is not running.");
    return 1;
}
catch (Exception e) when (e is InvalidSessionException || (e is UnknownUserException ex && ex.unknown == "::MumbleServer::InvalidSessionException"))
{
    Console.WriteLine("Session not found on virtual server.");
    return 1;
}
catch (Exception e) when (e is InvalidChannelException || (e is UnknownUserException ex && ex.unknown == "::MumbleServer::InvalidChannelException"))
{
    Console.WriteLine("Channel not found on virtual server.");
    return 1;
}
catch (MumbleServerCliException e)
{
    Console.WriteLine(e.Message);
    return 1;
}
catch (UnknownUserException e)
{
    Console.WriteLine("The Mumble Ice interface is throwing an exception not declared in the Ice file. Ask a Mumble developer to fix this!");
    Console.WriteLine($"Exception thrown: {e.unknown}");
    return 1;
}
