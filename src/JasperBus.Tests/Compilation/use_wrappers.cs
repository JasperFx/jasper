using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using JasperBus.Model;
using JasperBus.Tests.Runtime;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Compilation
{
    public class use_wrappers : CompilationContext<TransactionalHandler>
    {
        private readonly Tracking theTracking = new Tracking();

        public use_wrappers()
        {
            services.For<Tracking>().Use(theTracking);
            services.ForSingletonOf<IFakeStore>().Use<FakeStore>();

            
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
    }

    public class TransactionalHandler
    {
        [FakeTransaction]
        public void Handle(Message1 message)
        {
            
        }
    }

    public class Tracking
    {
        public bool DisposedTheSession;
        public bool OpenedSession;
        public bool CalledSaveChanges;
    }

    public class FakeTransactionAttribute : ModifyHandlerChainAttribute
    {
        public override void Modify(HandlerChain chain)
        {
            chain.Wrappers.Add(new FakeTransaction());
        }
    }

    public class FakeTransaction : Frame
    {
        private Variable _store;
        private readonly Variable _session;

        public FakeTransaction() : base(false)
        {
            _session = new Variable(typeof(IFakeSession), "session", this);
        }

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            _store = chain.FindVariable(typeof(IFakeStore));
            yield return _store;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"BLOCK:using (var {_session.Usage} = {_store.Usage}.OpenSession())");
            Next?.GenerateCode(method, writer);
            writer.Write($"{_session.Usage}.{nameof(IFakeSession.SaveChanges)}();");
            writer.FinishBlock();
        }
    }

    public interface IFakeStore
    {
        IFakeSession OpenSession();
    }

    public class FakeStore : IFakeStore
    {
        private readonly Tracking _tracking;

        public FakeStore(Tracking tracking)
        {
            _tracking = tracking;
        }

        public IFakeSession OpenSession()
        {
            _tracking.OpenedSession = true;
            return new FakeSession(_tracking);
        }
    }

    public interface IFakeSession : IDisposable
    {
        void SaveChanges();
    }

    public class FakeSession : IFakeSession
    {
        private readonly Tracking _tracking;

        public FakeSession(Tracking tracking)
        {
            _tracking = tracking;
        }

        public void Dispose()
        {
            _tracking.DisposedTheSession = true;
        }

        public void SaveChanges()
        {
            _tracking.CalledSaveChanges = true;
        }
    }
}