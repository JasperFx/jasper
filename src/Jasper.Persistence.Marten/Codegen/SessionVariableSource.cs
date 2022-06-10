using System;
using Jasper.Persistence.Marten.Publishing;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Marten;

namespace Jasper.Persistence.Marten.Codegen;

public class SessionVariableSource : IVariableSource
{
    public bool Matches(Type type)
    {
        return type == typeof(IQuerySession) || type == typeof(IDocumentSession);
    }

    public Variable Create(Type type)
    {
        if (type == typeof(IQuerySession))
        {
            return MethodCall.For<ISessionFactory>(x => x.QuerySession()).ReturnVariable;
        }

        if (type == typeof(IDocumentSession))
        {
            return MethodCall.For<OutboxedSessionFactory>(x => x.OpenSession(null!)).ReturnVariable;
        }

        throw new ArgumentOutOfRangeException(nameof(type));
    }
}
