#region sample_BootstrappingPinger

using Jasper;
using Jasper.Transports.Tcp;
using Messages;
using Oakton;
using Pinger;

return await Host.CreateDefaultBuilder(args)
    .UseJasper(opts =>
    {
        // Using Jasper's built in TCP transport

        // listen to incoming messages at port 5580
        opts.ListenAtPort(5580);

        // route all Ping messages to port 5581
        opts.PublishMessage<Ping>().ToPort(5581);

        // Registering the hosted service here, but could do
        // that with a separate call to IHostBuilder.ConfigureServices()
        opts.Services.AddHostedService<Worker>();
    })
    .RunOaktonCommands(args);

#endregion

