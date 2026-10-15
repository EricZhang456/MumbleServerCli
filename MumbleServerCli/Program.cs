using MumbleServer;
using System.CommandLine;
using MumbleServerCli.Action;

var rootCommand = new RootCommand("CLI program to control mumble-server via the Ice interface.");
var connstrOpt = new Option<string>("--connstr")
{
    Description = "Connection string to Mumble Ice endpoint.",
    DefaultValueFactory = _ => "tcp -h 127.0.0.1 -p 6502 -t 60000"
};

rootCommand.Options.Add(connstrOpt);

rootCommand.SetAction(result =>
{
    using var communicator = Ice.Util.initialize(ref args);
    var obj = communicator.stringToProxy("Meta:" + result.GetValue(connstrOpt));
    var meta = MetaPrxHelper.checkedCast(obj);
    if (meta is null)
    {
        throw new SystemException("Provided Ice server is not a Mumble server");
    }

    Router.RegisterActions(rootCommand, meta);
});

var parseResult = rootCommand.Parse(args);
using var communicator = Ice.Util.initialize(ref args);

parseResult.Invoke();
