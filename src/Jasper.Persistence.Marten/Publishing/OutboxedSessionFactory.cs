using Jasper.Runtime;
using Marten;

namespace Jasper.Persistence.Marten.Publishing;

public class OutboxedSessionFactory
{
    private readonly ISessionFactory _factory;
    private readonly bool _shouldPublishEvents;

    public OutboxedSessionFactory(ISessionFactory factory, IJasperRuntime runtime)
    {
        _factory = factory;
        _shouldPublishEvents = runtime.TryFindExtension<MartenIntegration>()?.ShouldPublishEvents ?? false;
    }

    /// <summary>Build new instances of IQuerySession on demand</summary>
    /// <returns></returns>
    public IQuerySession QuerySession(IExecutionContext context)
    {
        return _factory.QuerySession();
    }

    /// <summary>Build new instances of IDocumentSession on demand</summary>
    /// <returns></returns>
    public IDocumentSession OpenSession(IExecutionContext context)
    {
        var session = _factory.OpenSession();
        context.StartTransaction(new MartenEnvelopeOutbox(session, context));

        if (_shouldPublishEvents)
        {
            session.Listeners.Add(new PublishIncomingEventsBeforeCommit(context));
        }

        session.Listeners.Add(new FlushOutgoingMessagesOnCommit(context));

        return session;
    }
}
