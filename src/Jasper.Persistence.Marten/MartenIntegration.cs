using System;
using System.Collections.Generic;
using Baseline.ImTools;
using Jasper.Configuration;
using Jasper.Persistence.Marten.Codegen;
using Jasper.Persistence.Marten.Persistence.Sagas;
using Jasper.Persistence.Sagas;
using Jasper.Runtime.Routing;
using Marten.Events;

namespace Jasper.Persistence.Marten;

internal class MartenIntegration : IJasperExtension
{
    public void Configure(JasperOptions options)
    {
        options.Advanced.CodeGeneration.Sources.Add(new MartenBackedPersistenceMarker());

        var frameProvider = new MartenSagaPersistenceFrameProvider();
        options.Advanced.CodeGeneration.SetSagaPersistence(frameProvider);
        options.Advanced.CodeGeneration.SetTransactions(frameProvider);

        options.Advanced.CodeGeneration.Sources.Add(new SessionVariableSource());
    }

    // This is set by additional fluent interface helpers
    public Action<IEventPublishingOptions>? PublishingConfiguration { get; set; }

    public List<Func<Type, bool>> IncludedEventsForPublishing { get; } = new();

}


public interface IEventPublishingOptions
{
    IPublishToExpression PublishAllEvents();
    IPublishToExpression PublishEvents(Func<Type, bool> filter);
    //IPublishToExpression PublishEvents<T>(Func<IEvent<T>, bool> filter) where T : notnull;

    IPublishToExpression PublishEvents<T>();
}


