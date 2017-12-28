using System;
using System.Collections.Generic;
using Jasper.Internals.IoC;

namespace Jasper.Internals.Codegen
{
    /// <summary>
    /// Models a logical method and how to find candidate variables
    /// </summary>
    public interface IMethodVariables
    {
        Variable FindVariable(Type type);
        Variable FindVariableByName(Type dependency, string name);
        bool TryFindVariableByName(Type dependency, string name, out Variable variable);
        Variable TryFindVariable(Type type, VariableSource source);

        IList<BuildStep> AllKnownBuildSteps { get; }

    }
}
