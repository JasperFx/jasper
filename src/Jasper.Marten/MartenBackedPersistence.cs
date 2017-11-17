using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Configuration;
using Jasper.Marten.Persistence;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Marten
{
    /// <summary>
    /// Opts into using Marten as the backing message store
    /// </summary>
    public class MartenBackedPersistence : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.AddSingleton<IPersistence, MartenBackedMessagePersistence>();
            registry.Settings.Alter<StoreOptions>(options =>
            {
                options.Schema.For<Envelope>().Duplicate(x => x.ExecutionTime);
            });
        }
    }
}
