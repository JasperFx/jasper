using System;
using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Jasper.Testing.Codegen.IoC;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Codegen
{
    public class HandlerGenerationTests: CompilationContext
    {
        private AsyncMode theAsyncMode => new HandlerGeneration(theChain, new GenerationConfig("Something"), "input")
            .AsyncMode;

        [Fact]
        public void detects_that_it_is_all_sync()
        {
            theChain.Call<MainInput>(x => x.DoSync());

            theAsyncMode.ShouldBe(AsyncMode.ReturnCompletedTask);

            theChain.Call<MainInput>(x => x.Touch());

            theAsyncMode.ShouldBe(AsyncMode.ReturnCompletedTask);
        }

        [Fact]
        public void detects_chain_as_async_task_if_multiple_asyncs()
        {
            theChain.Call<MainInput>(x => x.DoSync());
            theChain.Call<MainInput>(x => x.TouchAsync());
            theChain.Call<MainInput>(x => x.DifferentAsync());

            theAsyncMode.ShouldBe(AsyncMode.AsyncTask);
        }

        [Fact]
        public void use_async_task_if_one_async_that_is_not_last()
        {
            theChain.Call<MainInput>(x => x.DoSync());
            theChain.Call<MainInput>(x => x.TouchAsync());

            theAsyncMode.ShouldBe(AsyncMode.ReturnFromLastNode);
        }

        [Fact]
        public void find_variable_caches_the_values()
        {
            Config.Sources.Add(new FakeSource());

            var generation = new HandlerGeneration(theChain, Config, "input");

            var variable1 = generation.FindVariable(typeof(ITouchService));
            var variable2 = generation.FindVariable(typeof(ITouchService));

            variable1.ShouldBeTheSameAs(variable2);
        }

        public class FakeSource : IVariableSource
        {
            public bool Matches(Type type)
            {
                return true;
            }

            public Variable Create(Type type)
            {
                return new FakeVariable(type);
            }
        }

        public class FakeVariable : Variable
        {
            public FakeVariable(Type argType) : base(argType)
            {
            }
        }



        [Fact]
        public void can_find_variables_built_by_one_of_the_existing_frames()
        {
            theChain.Call<MainInput>(x => x.DoSync());
            theChain.AddToEnd(new CreatingFrame());

            var generation = new HandlerGeneration(theChain, Config, "input");

            generation.FindVariable(typeof(ITouchService))
                .ShouldBeOfType<FakeVariable>();
        }

        [Fact]
        public void can_find_input_variable()
        {
            var generation = new HandlerGeneration(theChain, Config, "input");

            generation.FindVariable(typeof(MainInput))
                .ShouldBeSameAs(generation);
        }
    }

    public class CreatingFrame : Frame
    {
        private readonly HandlerGenerationTests.FakeVariable _variable
            = new HandlerGenerationTests.FakeVariable(typeof(ITouchService));

        public CreatingFrame() : base(false)
        {

        }

        public override IEnumerable<Variable> Creates
        {
            get { yield return _variable; }
        }


        public override void GenerateCode(HandlerGeneration generation, ISourceWriter writer)
        {
            Next?.GenerateAllCode(generation, writer);
        }
    }
}