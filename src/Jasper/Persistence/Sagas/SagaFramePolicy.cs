using System;
using System.Linq;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Runtime.Handlers;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Sagas;

internal class SagaFramePolicy
{
    public const string SagaIdVariableName = "sagaId";
    public static readonly Type[] ValidSagaIdTypes = { typeof(Guid), typeof(int), typeof(long), typeof(string) };

}
