using System;
using System.Collections.Generic;
using Jasper.Internals.Compilation;
using Jasper.Internals.IoC;
using Jasper.Internals.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Internals.Codegen.ServiceLocation
{


    public class ServiceVariable : Variable
    {
        public ServiceVariable(Type variableType, string usage, ServiceCreationFrame creator) : base(variableType, usage, creator)
        {

        }

        public ServiceVariable(Type variableType, ServiceCreationFrame creator) : base(variableType, creator)
        {
        }

        public bool CanBeReduced => Creator.As<ServiceCreationFrame>().CanBeReduced;

        public void UseInlinePlan()
        {
            if (!(Creator is ServiceCreationFrame)) return;

            var inner = Creator.As<ServiceCreationFrame>().BuildStepPlannerVariable;
            Usage = inner.Usage;
            Creator = inner.Creator;
        }
    }

    public class ServiceCreationFrame : SyncFrame
    {
        private readonly BuildStepPlanner _planner;
        private Variable _provider;

        public ServiceCreationFrame(Type serviceType, BuildStepPlanner planner)
        {
            _planner = planner;
            Service = new ServiceVariable(serviceType, this);
        }

        public Variable Service { get; }
        public bool CanBeReduced => _planner?.CanBeReduced ?? false;

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _provider = chain.FindVariable(typeof(IServiceProvider));
            yield return _provider;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var typeFullName = Service.VariableType.FullName.Replace("+", ".");
            var declaration = $"var {Service.Usage} = ({typeFullName}){_provider.Usage}.{nameof(IServiceProvider.GetService)}(typeof({typeFullName}))";

            if (Service.VariableType.CanBeCastTo<IDisposable>())
            {
                writer.UsingBlock(declaration, w => Next?.GenerateCode(method, w));
            }
            else
            {
                writer.Write(declaration + ";");
                Next?.GenerateCode(method, writer);
            }
        }

        public Variable BuildStepPlannerVariable => _planner.Top.Variable;
    }
}
