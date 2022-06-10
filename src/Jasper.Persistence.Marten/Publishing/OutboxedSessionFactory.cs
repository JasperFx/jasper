using Marten;

namespace Jasper.Persistence.Marten.Publishing;

public class OutboxedSessionFactory
{
    private readonly ISessionFactory _factory;

    public OutboxedSessionFactory(ISessionFactory factory)
    {
        _factory = factory;
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
        // TODO -- alternatively put in a listener for event publishing???

        session.Listeners.Add(new FlushOutgoingMessagesOnCommit(context));


        return session;
    }
}
