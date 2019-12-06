using System;
using Jasper.Persistence.Postgresql;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Marten
{
    internal class MartenBackedPersistenceMarker : IVariableSource
    {
        public bool Matches(Type type)
        {
            return type == GetType();
        }

        public Variable Create(Type type)
        {
            return Variable.For<PostgresqlEnvelopePersistence>();
        }
    }
}