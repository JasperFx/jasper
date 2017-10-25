using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Configuration;
using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;
using Jasper.Testing.Bus.Runtime;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Compilation
{
    public class use_wrappers : CompilationContext
    {
        private readonly FakeStoreTypes.Tracking theTracking = new FakeStoreTypes.Tracking();

        public use_wrappers()
        {
            theRegistry.Handlers.IncludeType<TransactionalHandler>();

            theRegistry.Services.AddSingleton(theTracking);
            theRegistry.Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();


        }


        [Fact]
        public async Task wrapper_executes()
        {
            var message = new Message1();

            await Execute(message);

            theTracking.DisposedTheSession.ShouldBeTrue();
            theTracking.OpenedSession.ShouldBeTrue();
            theTracking.CalledSaveChanges.ShouldBeTrue();
        }

        [Fact]
        public async Task wrapper_applied_by_generic_attribute_executes()
        {
            var message = new Message2();

            await Execute(message);

            theTracking.DisposedTheSession.ShouldBeTrue();
            theTracking.OpenedSession.ShouldBeTrue();
            theTracking.CalledSaveChanges.ShouldBeTrue();
        }
    }

    [JasperIgnore]
    public class TransactionalHandler
    {
        [FakeTransaction]
        public void Handle(Message1 message)
        {

        }

        [GenericFakeTransaction]
        public void Handle(Message2 message)
        {

        }
    }






}
