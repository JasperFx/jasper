using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Marten.Codegen;
using Jasper.Testing.Bus.Runtime;
using Marten;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests
{
    public class transactional_frame_end_to_end
    {
        [Fact]
        public async Task the_transactional_middleware_works()
        {
            using (var runtime = JasperRuntime.For<MartenUsingApp>())
            {
                var command = new CreateDocCommand();
                await runtime.Bus.Invoke(command);

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
        [MartenTransaction]
        public void Handle(CreateDocCommand message, IDocumentSession session)
        {
            session.Store(new FakeDoc{Id = message.Id});
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
        public void Apply(HandlerGraph graph)
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
    public class CommandsAreTransactionalApp : JasperRegistry
    {
        public CommandsAreTransactionalApp()
        {
            // And actually use the policy
            Handlers.GlobalPolicy<CommandsAreTransactional>();
        }
    }
    // ENDSAMPLE

}
