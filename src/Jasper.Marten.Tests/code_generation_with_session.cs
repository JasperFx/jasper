using Jasper.Messaging.Model;
using Marten;
using Servers;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests
{
    public class code_generation_with_session
    {
        private string codeFor<THandler, TMessage>()
        {
            var code = "";

            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Settings.MartenConnectionStringIs(MartenContainer.ConnectionString);

                _.Include<MartenBackedPersistence>();

                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<THandler>();
            }))
            {
                code = runtime.Get<HandlerGraph>().ChainFor<TMessage>().SourceCode;
            }

            return code;
        }

        [Fact]
        public void default_session_construction_no_transaction()
        {
            var code = codeFor<SessionUsingBlock1, Message1>();

            code.ShouldContain("using (var documentSession = _documentStore.LightweightSession())");
            code.ShouldNotContain(
                "await Jasper.Marten.MessageContextExtensions.EnlistInTransaction(context, documentSession);");
        }

        [Fact]
        public void default_session_construction_with_transaction()
        {
            var code = codeFor<SessionUsingBlock1, Message2>();

            code.ShouldContain("using (var documentSession = _documentStore.LightweightSession())");
            code.ShouldContain(
                "await Jasper.Marten.MessageContextExtensions.EnlistInTransaction(context, documentSession);");
        }

        [Fact]
        public void override_session_construction_no_transaction()
        {
            var code = codeFor<SessionUsingBlock2, Message1>();

            code.ShouldNotContain("using (var documentSession = _documentStore.LightweightSession())");
            code.ShouldContain("using (var documentSession = sessionUsingBlock2.OpenSession(_documentStore))");
            code.ShouldNotContain(
                "await Jasper.Marten.MessagingExtensions.EnlistInTransaction(context, documentSession);");
        }

        [Fact]
        public void override_session_construction_with_transaction()
        {
            var code = codeFor<SessionUsingBlock2, Message2>();

            code.ShouldNotContain("using (var documentSession = _documentStore.LightweightSession())");
            code.ShouldContain("using (var documentSession = sessionUsingBlock2.OpenSession(_documentStore))");
            code.ShouldContain(
                "await Jasper.Marten.MessageContextExtensions.EnlistInTransaction(context, documentSession);");
        }
    }

    public class SessionUsingBlock1
    {
        public void Consume(Message1 message, IDocumentSession session)
        {
        }

        [MartenTransaction]
        public void Consume(Message2 message, IDocumentSession session)
        {
        }
    }

    // SAMPLE: custom-marten-session-creation
    public class SessionUsingBlock2
    {
        // This method will be used to create the IDocumentSession
        // that will be used by the two Consume() methods below
        public IDocumentSession OpenSession(IDocumentStore store)
        {
            // Here I'm opting to use the heavier,
            // automatic dirty state checking
            return store.DirtyTrackedSession();
        }


        public void Consume(Message1 message, IDocumentSession session)
        {
        }

        [MartenTransaction]
        public void Consume(Message2 message, IDocumentSession session)
        {
        }
    }

    // ENDSAMPLE
}
