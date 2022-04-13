using IntegrationTests;
using Jasper.Attributes;
using Jasper.Persistence.Marten;
using Jasper.Runtime.Handlers;
using Marten;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class code_generation_with_session : PostgresqlContext
    {
        private string codeFor<THandler, TMessage>()
        {
            var code = "";

            using var runtime = JasperHost.For(opts =>
            {
                opts.Services.AddMarten(Servers.PostgresConnectionString)
                    .IntegrateWithJasper();

                opts.Handlers.DisableConventionalDiscovery();
                opts.Handlers.IncludeType<THandler>();
            });

            code = runtime.Get<HandlerGraph>().ChainFor<TMessage>().SourceCode;

            return code;
        }

        [Fact]
        public void default_session_construction_no_transaction()
        {
            var code = codeFor<SessionUsingBlock1, Message1>();

            code.ShouldContain("using var documentSession = _sessionFactory.OpenSession();");
            code.ShouldNotContain(
                "await Jasper.Persistence.Marten.MessageContextExtensions.EnlistInTransaction(context, documentSession);");
        }

        [Fact]
        public void default_session_construction_with_transaction()
        {
            var code = codeFor<SessionUsingBlock1, Message2>();

            code.ShouldContain("using var documentSession = _sessionFactory.OpenSession();");
            code.ShouldContain(
                "await Jasper.Persistence.Marten.ExecutionContextExtensions.EnlistInTransactionAsync(context, documentSession);");
        }

    }

    public class SessionUsingBlock1
    {
        public void Consume(Message1 message, IDocumentSession session)
        {
        }

        [Transactional]
        public void Consume(Message2 message, IDocumentSession session)
        {
        }
    }


}
