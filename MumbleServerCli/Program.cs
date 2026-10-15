using MumbleServer;
using System.CommandLine;
using Ice;
using Router = MumbleServerCli.Action.Router;
using SystemException = System.SystemException;
using TimeoutException = Ice.TimeoutException;

const string CONN_STR_ENVVAR = "MUMBLE_SERVER_CLI_CONN_STR";
const string SECRET_ENV_VAR = "MUMBLE_SERVER_CLI_SECRET";

var warned = false;
var rootCommand = new RootCommand("CLI program to control mumble-server via the Ice interface.");
var connStr = Environment.GetEnvironmentVariable(CONN_STR_ENVVAR);
if (string.IsNullOrWhiteSpace(connStr))
{
    Console.WriteLine($"Warning: {CONN_STR_ENVVAR} not set. Defaulting to \"tcp -h 127.0.0.1 -p 6502 -t 60000\"");
    connStr = "tcp -h 127.0.0.1 -p 6502 -t 60000";
    warned = true;
}

var secret = Environment.GetEnvironmentVariable(SECRET_ENV_VAR);
var secretSet = !string.IsNullOrWhiteSpace(secret);
if (!secretSet)
{
    Console.WriteLine($"Warning: {SECRET_ENV_VAR} not set.");
    warned = true;
}

if (warned)
{
    Console.WriteLine();
}

var props = Util.createProperties();
props.setProperty("Ice.ImplicitContext", "Shared");

var initData = new InitializationData();
initData.properties = props;

using var communicator = Ice.Util.initialize(initData);
if (secretSet)
{
    communicator.getImplicitContext().put("secret", secret);
}
var obj = communicator.stringToProxy("Meta:" + connStr);
var meta = MetaPrxHelper.checkedCast(obj);
if (meta is null)
{
    throw new SystemException("Provided Ice server is not a Mumble server");
}

Router.RegisterActions(rootCommand, meta);

var parseResult = rootCommand.Parse(args);

var invokeConfig = new InvocationConfiguration
{
    EnableDefaultExceptionHandler = false
};
try
{
    parseResult.Invoke(invokeConfig);
}
catch (ConnectionRefusedException)
{
    Console.WriteLine("Connection refused.");
}
catch (TimeoutException)
{
    Console.WriteLine("Connection timed out.");
}
catch (InvalidSecretException)
{
    Console.WriteLine($"Invalid secret. Set {SECRET_ENV_VAR} to the correct secret.");
}
