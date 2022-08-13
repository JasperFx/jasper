using System.Runtime.CompilerServices;
using Jasper.Attributes;
using Lamar;
using Oakton;

[assembly: IgnoreAssembly]
[assembly: BaselineTypeDiscovery.IgnoreAssembly]
[assembly: OaktonCommandAssembly]
[assembly: JasperFeature]

[assembly: InternalsVisibleTo("Jasper.Testing")]
[assembly: InternalsVisibleTo("CircuitBreakingTests")]
[assembly: InternalsVisibleTo("TestingSupport")]
[assembly: InternalsVisibleTo("Jasper.RabbitMq")]
[assembly: InternalsVisibleTo("Jasper.Http")]
[assembly: InternalsVisibleTo("Jasper.RabbitMq.Tests")]
[assembly: InternalsVisibleTo("Jasper.AzureServiceBus")]
[assembly: InternalsVisibleTo("Jasper.ConfluentKafka")]
[assembly: InternalsVisibleTo("Jasper.AzureServiceBus.Tests")]
[assembly: InternalsVisibleTo("Jasper.Persistence.Testing")]
[assembly: InternalsVisibleTo("Jasper.Persistence.Database")]
[assembly: InternalsVisibleTo("Jasper.Persistence.Marten")]
[assembly: InternalsVisibleTo("Jasper.Persistence.EntityFrameworkCore")]
[assembly: InternalsVisibleTo("Jasper.Pulsar")]
[assembly: InternalsVisibleTo("Jasper.Pulsar.Tests")]
[assembly: InternalsVisibleTo("StorytellerSpecs")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
