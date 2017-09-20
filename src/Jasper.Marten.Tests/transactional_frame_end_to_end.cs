using System;
using System.Threading.Tasks;
using Jasper;
using Marten;
using Shouldly;
using Xunit;

namespace JasperBus.Marten.Tests
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



    public class CreateDocCommandHandler
    {
        [MartenTransaction]
        public void Handle(CreateDocCommand message, IDocumentSession session)
        {
            session.Store(new FakeDoc{Id = message.Id});
        }
    }
}
