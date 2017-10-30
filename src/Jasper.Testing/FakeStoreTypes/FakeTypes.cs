using System;
using System.Collections.Generic;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Configuration;
using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;
using Jasper.Testing.Http;

namespace Jasper.Testing.FakeStoreTypes
{
    public class GenericFakeTransactionAttribute : ModifyChainAttribute
    {
        public override void Modify(IChain chain)
        {
            chain.Middleware.Add(new FakeTransaction());
        }
    }

    public class FakeTransactionAttribute : ModifyHandlerChainAttribute
    {
        public override void Modify(HandlerChain chain)
        {
            chain.Middleware.Add(new FakeTransaction());
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

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
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

    public class Tracking
    {
        public bool DisposedTheSession;
        public bool OpenedSession;
        public bool CalledSaveChanges;
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
