using System;
using MumbleServer;
using System.CommandLine;
using MumbleServerCli.Action;

const string CONN_STR_ENVVAR = "MUMBLE_SERVER_CLI_CONN_STR";
const string SECRET_ENV_VAR = "MUMBLE_SERVER_CLI_SECRET";

var rootCommand = new RootCommand("CLI program to control mumble-server via the Ice interface.");
var connStr = Environment.GetEnvironmentVariable(CONN_STR_ENVVAR);
if (string.IsNullOrWhiteSpace(connStr))
{
    Console.WriteLine($"Warning: {CONN_STR_ENVVAR} not set. Defaulting to \"tcp -h 127.0.0.1 -p 6502 -t 60000\"");
    connStr = "tcp -h 127.0.0.1 -p 6502 -t 60000";
}

var secret = Environment.GetEnvironmentVariable(SECRET_ENV_VAR);
var secretSet = !string.IsNullOrWhiteSpace(secret);
if (!secretSet)
{
    Console.WriteLine($"Warning: {SECRET_ENV_VAR} not set.");
}

using var communicator = Ice.Util.initialize(ref args);
var obj = communicator.stringToProxy("Meta:" + connStr);
var meta = MetaPrxHelper.checkedCast(obj);
if (meta is null)
{
    throw new SystemException("Provided Ice server is not a Mumble server");
}

if (secretSet)
{
    var context = new Dictionary<string, string>
    {
        { "secret", secret }
    };
    meta = (MetaPrx)meta.ice_context(context);
}

Router.RegisterActions(rootCommand, meta);

var parseResult = rootCommand.Parse(args);
parseResult.Invoke();
