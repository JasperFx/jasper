using Jasper.Configuration;
using Jasper.Persistence.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.Testing.Marten
{
    // SAMPLE: AppUsingMartenMessagePersistence
    public class AppUsingMartenMessagePersistence : JasperOptions
    {
        public AppUsingMartenMessagePersistence()
        {
            // Use a "durable" TCP listener at port
            // 2222 where the incoming messages will be
            // persisted with Marten upon receipt and
            // deleted only when the message is successfully
            // processed
            Endpoints.ListenAtPort(2222).Durably();
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // Use this line to activate the Marten-backed
            // message persistence for durable, store and forward
            // messaging
            Extensions.UseMarten(storeOptions =>
            {
                storeOptions.Connection(config["marten_database"]);

                // Other Marten configuration against

            });
        }
    }

    // ENDSAMPLE
}
