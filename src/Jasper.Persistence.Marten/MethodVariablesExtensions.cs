using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Marten;

internal static class MethodVariablesExtensions
{
    internal static bool IsUsingMartenPersistence(this IMethodVariables method)
    {
        return method.TryFindVariable(typeof(MartenBackedPersistenceMarker), VariableSource.NotServices) != null;
    }
}
