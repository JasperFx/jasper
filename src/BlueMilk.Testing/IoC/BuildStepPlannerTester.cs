using System;
using System.Collections.Generic;
using Jasper.Internals;
using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;
using Jasper.Internals.IoC;
using Jasper.Testing.Internals.TargetTypes;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Internals.IoC
{
    public class CommentFrame : SyncFrame
    {
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write("// Kilroy was here.");
            Next?.GenerateCode(method, writer);
        }
    }

    public class StubMethodVariables : IMethodVariables
    {
        public readonly Dictionary<Type, Variable> Variables = new Dictionary<Type, Variable>();

        public IList<BuildStep> AllKnownBuildSteps { get; } = new List<BuildStep>();

        public Variable FindVariable(Type type)
        {
            throw new NotImplementedException();
        }

        public Variable FindVariableByName(Type dependency, string name)
        {
            throw new NotImplementedException();
        }

        public bool TryFindVariableByName(Type dependency, string name, out Variable variable)
        {
            throw new NotImplementedException();
        }

        public Variable TryFindVariable(Type type, VariableSource source)
        {
            return Variables.ContainsKey(type) ? Variables[type] : null;
        }
    }

    public class BuildStepPlannerTester
    {
        public readonly ServiceRegistry theServices = new ServiceRegistry();
        private ServiceGraph theGraph;
        private StubMethodVariables theMethod = new StubMethodVariables();

        public BuildStepPlannerTester()
        {
            theGraph = new ServiceGraph(theServices);
        }

        private BuildStepPlanner executePlan<T>()
        {
            return new BuildStepPlanner(typeof(T), typeof(T), theGraph, theMethod);
        }

        [Fact]
        public void not_reduceable_if_no_public_ctors()
        {
            executePlan<NoPublicCtorGuy>()
                .CanBeReduced.ShouldBeFalse();
        }

        [Fact]
        public void not_reduceable_if_cannot_resolve_all()
        {
            executePlan<WidgetUsingGuy>()
                .CanBeReduced.ShouldBeFalse();
        }

        [Fact]
        public void reduceable_if_can_be_built_with_services()
        {
            theServices.AddTransient<IWidget, AWidget>();

            executePlan<WidgetUsingGuy>()
                .CanBeReduced.ShouldBeTrue();
        }

    }

    public class WidgetUsingGuy
    {
        public WidgetUsingGuy(IWidget widget)
        {
        }
    }

    public class NoPublicCtorGuy
    {
        private NoPublicCtorGuy()
        {
        }
    }
}
