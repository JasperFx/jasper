using Jasper;
using Jasper.Persistence.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SqlReceiver
{
    public class ReceiverApp : JasperRegistry
    {
        public ReceiverApp()
        {

            Hosting(x => x.UseUrls("http://*:5061").UseKestrel());


            Settings.PersistMessagesWithSqlServer((context, settings) =>
            {
                settings.SchemaName = "receiver";
                settings.ConnectionString = context.Configuration["mssql"];
            });


            Transports.ListenForMessagesFromUriValueInConfig("listener");
        }
    }
}
