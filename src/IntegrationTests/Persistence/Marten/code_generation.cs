using System;
using Jasper;
using Jasper.Messaging.Model;
using Jasper.Persistence.Marten;
using Marten;
using Servers.Docker;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.Marten
{
    public class code_generation : MartenContext, IDisposable
    {
        public code_generation(DockerFixture<MartenContainer> fixture) : base(fixture)
        {

            runtime = JasperRuntime.For<MartenUsingApp>();
        }

        public void Dispose()
        {
            runtime?.Dispose();
        }

        private readonly JasperRuntime runtime;

        [Fact]
        public void codegen_document_session_creation()
        {
            var handlerGraph = runtime.Get<HandlerGraph>();
            var messageHandler = handlerGraph.HandlerFor<CreateFakeDoc>();
            messageHandler
                .Chain.SourceCode.ShouldContain("var documentSession = _documentStore.LightweightSession()");
        }


        [Fact]
        public void codegen_query_session_creation()
        {
            var handlerGraph = runtime.Get<HandlerGraph>();
            var messageHandler = handlerGraph.HandlerFor<LookupFakeDoc>();
            messageHandler
                .Chain.SourceCode.ShouldContain("var querySession = _documentStore.QuerySession()");
        }

        [Fact]
        public void codegen_transactional_session_usage()
        {
            var sourceCode = runtime
                .Get<HandlerGraph>()
                .HandlerFor<Command>()
                .Chain
                .SourceCode;

            sourceCode.ShouldContain("var documentSession = _documentStore.LightweightSession()");
            sourceCode.ShouldContain("await documentSession.SaveChangesAsync()");
        }
    }

    public class FakeDocEventHandler
    {
        public void Handle(LookupFakeDoc message, IQuerySession session)
        {
        }

        public void Handle(CreateFakeDoc message, IDocumentSession session)
        {
        }

        [MartenTransaction]
        public void Handle(Command message, IDocumentSession session)
        {
        }
    }

    public class Command
    {
    }

    public class LookupFakeDoc
    {
        public Guid DocId = Guid.NewGuid();
    }

    public class CreateFakeDoc
    {
        public Guid DocId = Guid.NewGuid();
    }
}
