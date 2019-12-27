using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Persistence.Marten.Codegen;
using Jasper.Runtime.Handlers;
using Lamar;
using LamarCodeGeneration;
using Marten;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class transactional_frame_end_to_end : PostgresqlContext
    {
        [Fact]
        public async Task the_transactional_middleware_works()
        {
            using (var runtime = JasperHost.For<MartenUsingApp>())
            {
                var command = new CreateDocCommand();
                await runtime.Invoke(command);

                using (var query = runtime.Get<IQuerySession>())
                {
                    query.Load<FakeDoc>(command.Id)
                        .ShouldNotBeNull();
                }
            }
        }
    }

    public class CreateDocCommand
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }


    // SAMPLE: CreateDocCommandHandler
    public class CreateDocCommandHandler
    {
        [Transactional]
        public void Handle(CreateDocCommand message, IDocumentSession session)
        {
            session.Store(new FakeDoc {Id = message.Id});
        }
    }
    // ENDSAMPLE

    // SAMPLE: UsingDocumentSessionHandler
    public class UsingDocumentSessionHandler
    {
        // Take in IDocumentStore as a constructor argument
        public UsingDocumentSessionHandler(IDocumentStore store)
        {
        }

        // Take in IDocumentSession as an argument
        public void Handle(Message1 message, IDocumentSession session)
        {
        }
    }
    // ENDSAMPLE

    // SAMPLE: CommandsAreTransactional
    public class CommandsAreTransactional : IHandlerPolicy
    {
        public void Apply(HandlerGraph graph, GenerationRules rules, IContainer container)
        {
            // Important! Create a brand new TransactionalFrame
            // for each chain
            graph
                .Chains
                .Where(x => x.MessageType.Name.EndsWith("Command"))
                .Each(x => x.Middleware.Add(new TransactionalFrame()));
        }
    }
    // ENDSAMPLE

    // SAMPLE: Using-CommandsAreTransactional
    public class CommandsAreTransactionalApp : JasperOptions
    {
        public CommandsAreTransactionalApp()
        {
            // And actually use the policy
            Handlers.GlobalPolicy<CommandsAreTransactional>();
        }
    }

    // ENDSAMPLE
}
