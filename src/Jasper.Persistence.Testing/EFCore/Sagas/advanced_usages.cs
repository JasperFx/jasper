using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Persistence.Testing.EFCore.Sagas
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

        public advanced_usages(ITestOutputHelper output) : base(output)
        {
        }
    }
}
