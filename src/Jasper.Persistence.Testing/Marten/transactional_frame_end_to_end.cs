using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Codegen;
using Jasper.Runtime.Handlers;
using Lamar;
using LamarCodeGeneration;
using Marten;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Weasel.Core;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class transactional_frame_end_to_end : PostgresqlContext
    {
        [Fact]
        public async Task the_transactional_middleware_works()
        {
            using var host = JasperHost.For(opts =>
            {
                opts.Services.AddMarten(o =>
                {
                    o.Connection(Servers.PostgresConnectionString);
                    o.AutoCreateSchemaObjects = AutoCreate.All;
                }).IntegrateWithJasper();
            });

            var command = new CreateDocCommand();
            await host.InvokeAsync(command);

            await using var query = host.Get<IQuerySession>();
            query.Load<FakeDoc>(command.Id)
                .ShouldNotBeNull();
        }

        public static async Task Using_CommandsAreTransactional()
                {
                    #region sample_Using_CommandsAreTransactional

                    using var host = await Host.CreateDefaultBuilder()
                        .UseJasper(opts =>
                        {
                            // And actually use the policy
                            opts.Handlers.GlobalPolicy<CommandsAreTransactional>();
                        }).StartAsync();

                    #endregion
                }
    }

    public class CreateDocCommand
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }


    #region sample_CreateDocCommandHandler
    public class CreateDocCommandHandler
    {
        [Transactional]
        public void Handle(CreateDocCommand message, IDocumentSession session)
        {
            session.Store(new FakeDoc {Id = message.Id});
        }
    }
    #endregion

    #region sample_UsingDocumentSessionHandler
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
    #endregion

    #region sample_CommandsAreTransactional
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
    #endregion

}
