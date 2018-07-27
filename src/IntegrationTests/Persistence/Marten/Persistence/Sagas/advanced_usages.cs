using System;
using System.Threading.Tasks;
using Servers;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.Marten.Persistence.Sagas
{
    public class advanced_usages : SagaTestHarness<IntBasicWorkflow, IntWorkflowState>
    {
        private readonly int stateId = new Random().Next();

        [Fact]
        public async Task can_return_the_state_from_a_start_method_as_part_of_a_value_tuple()
        {
            await send(new StartAndDoThings {Id = stateId, Name = "Goblin"});

            var state = await LoadState(stateId);

            state.ShouldNotBeNull();
            state.Name.ShouldBe("Goblin");
        }

        public advanced_usages(DockerFixture<MartenContainer> fixture) : base(fixture)
        {
        }
    }
}
