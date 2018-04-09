using System;
using System.Threading.Tasks;
using Jasper.Testing.Messaging.Sagas;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Persistence.Sagas
{
    public class advanced_usages : SagaTestHarness<IntBasicWorkflow, IntWorkflowState>
    {
        private readonly int stateId = new Random().Next();

        //[Fact]
        public async Task can_return_the_state_from_a_start_method_as_part_of_a_value_tuple()
        {
            await send(new StartAndDoThings {Id = stateId, Name = "Goblin"});

            var state = LoadState(stateId);

            state.ShouldNotBeNull();
            state.Name.ShouldBe("Goblin");
        }
    }
}
