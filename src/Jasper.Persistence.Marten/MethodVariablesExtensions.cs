using Jasper.Configuration;
using Jasper.Persistence.Marten.Persistence.Sagas;
using LamarCodeGeneration.Model;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Persistence.Marten
{

    internal static class MethodVariablesExtensions
    {
        internal static bool IsUsingMartenPersistence(this IMethodVariables method)
        {
            return method.TryFindVariable(typeof(MartenBackedPersistenceMarker), VariableSource.NotServices) != null;
        }
    }
}
