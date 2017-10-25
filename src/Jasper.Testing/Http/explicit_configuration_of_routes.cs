using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Configuration;
using Jasper.Http.Model;
using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http
{
    public class explicit_configuration_of_routes
    {

        [Fact]
        public void applies_the_Configure_RoutedChain_method()
        {
            var chain = RouteChain.For<ConfiguredEndpoint>(x => x.get_configured());

            var frames = chain.DetermineFrames();

            frames.OfType<FakeTransaction>().Any().ShouldBeTrue();
        }

        [Fact]
        public void applies_the_Configure_IChain_method()
        {
            var chain = RouteChain.For<ConfiguredEndpoint>(x => x.get_configured());

            var frames = chain.DetermineFrames();

            frames.OfType<FakeWrapper>().Any().ShouldBeTrue();
        }

        [Fact]
        public void applies_attributes_against_the_RouteChain()
        {
            var chain = RouteChain.For<ConfiguredEndpoint>(x => x.get_wrapper2());

            var frames = chain.DetermineFrames();

            frames.OfType<FakeWrapper2>().Any().ShouldBeTrue();
        }

        [Fact]
        public void applies_attributes_against_the_IChain()
        {
            var chain = RouteChain.For<ConfiguredEndpoint>(x => x.get_wrapper3());

            var frames = chain.DetermineFrames();

            frames.OfType<FakeWrapper3>().Any().ShouldBeTrue();
        }
    }

    [JasperIgnore]
    public class ConfiguredEndpoint
    {
        public void get_configured()
        {

        }

        [FakeWrapper2]
        public void get_wrapper2()
        {

        }

        [FakeWrapper3]
        public void get_wrapper3()
        {

        }

        public static void Configure(RouteChain chain)
        {
            chain.Middleware.Add(new FakeTransaction());
        }

        public static void Configure(IChain chain)
        {
            chain.Middleware.Add(new FakeWrapper());
        }
    }


    public class FakeWrapper : Frame
    {
        public FakeWrapper() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {

        }
    }

    public class FakeWrapper2Attribute : ModifyRouteAttribute
    {
        public override void Modify(RouteChain chain)
        {
            chain.Middleware.Add(new FakeWrapper2());
        }
    }

    public class FakeWrapper3Attribute : ModifyChainAttribute
    {
        public override void Modify(IChain chain)
        {
            chain.Middleware.Add(new FakeWrapper3());
        }
    }

    public class FakeWrapper2 : FakeWrapper { }
    public class FakeWrapper3 : FakeWrapper { }






}
