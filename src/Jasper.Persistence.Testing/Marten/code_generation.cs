using System;
using IntegrationTests;
using Jasper.Attributes;
using Jasper.Persistence.Marten;
using Jasper.Runtime.Handlers;
using Marten;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Weasel.Core;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class code_generation : PostgresqlContext, IDisposable
    {
        public code_generation()
        {
            runtime = JasperHost.For(opts =>
            {
                opts.Services.AddMarten(o =>
                {
                    o.Connection(Servers.PostgresConnectionString);
                    o.AutoCreateSchemaObjects = AutoCreate.All;
                }).IntegrateWithJasper();
            });
        }

        public void Dispose()
        {
            runtime?.Dispose();
        }

        private readonly IHost runtime;

        [Fact]
        public void codegen_document_session_creation()
        {
            var handlerGraph = runtime.Get<HandlerGraph>();
            var messageHandler = handlerGraph.HandlerFor<CreateFakeDoc>();
            messageHandler
                .Chain.SourceCode.ShouldContain("using var documentSession = _sessionFactory.OpenSession();");
        }


        [Fact]
        public void codegen_query_session_creation()
        {
            var handlerGraph = runtime.Get<HandlerGraph>();
            var messageHandler = handlerGraph.HandlerFor<LookupFakeDoc>();
            messageHandler
                .Chain.SourceCode.ShouldContain("using var querySession = _sessionFactory.QuerySession();");
        }

        [Fact]
        public void codegen_transactional_session_usage()
        {
            var sourceCode = runtime
                .Get<HandlerGraph>()
                .HandlerFor<Command>()
                .Chain
                .SourceCode;

            sourceCode.ShouldContain("using var documentSession = _sessionFactory.OpenSession();");
            sourceCode.ShouldContain("await documentSession.SaveChangesAsync().ConfigureAwait(false)");
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

        [Transactional]
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
