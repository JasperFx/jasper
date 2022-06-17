#region sample_PongerBootstrapping

using Jasper;
using Jasper.Transports.Tcp;
using Microsoft.Extensions.Hosting;
using Oakton;

return await Host.CreateDefaultBuilder(args)
    .UseJasper(opts =>
    {
        // Using Jasper's built in TCP transport
        opts.ListenAtPort(5581);
    })
    .RunOaktonCommands(args);


#endregion
