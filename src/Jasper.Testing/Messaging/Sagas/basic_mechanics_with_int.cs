using System;
using System.Threading.Tasks;
using Jasper.Messaging.Sagas;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Sagas
{
    [JasperIgnore]
    public class IntBasicWorkflow : BasicWorkflow<IntWorkflowState, IntStart, IntCompleteThree, int>
    {
        public IntWorkflowState Starts(WildcardStart start)
        {
            var sagaId = int.Parse(start.Id);
            return new IntWorkflowState
            {
                Id = sagaId,
                Name = start.Name
            };
        }



        public void Handles(IntDoThree message, IntWorkflowState state)
        {
            state.ThreeCompleted = true;
        }

        public (IntWorkflowState, CompleteFour) Start(StartAndDoThings message)
        {
            return (new IntWorkflowState {Id = message.Id, Name = message.Name}, new CompleteFour());
        }
    }

    public class StartAndDoThings
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Whisper";
    }

    public class IntDoThree
    {
        [SagaIdentity]
        public int TheSagaId { get; set; }
    }

    public class basic_mechanics_with_int : SagaTestHarness<IntBasicWorkflow, IntWorkflowState>
    {
        private readonly int stateId = new Random().Next();

        [Fact]
        public async Task start_1()
        {
            await send(new IntStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            var state = LoadState(stateId);

            state.ShouldNotBeNull();
            state.Name.ShouldBe("Croaker");
        }

        [Fact]
        public async Task start_2()
        {
            await send(new WildcardStart
            {
                Id = stateId.ToString(),
                Name = "One Eye"
            });

            var state = LoadState(stateId);

            state.ShouldNotBeNull();
            state.Name.ShouldBe("One Eye");
        }

        [Fact]
        public async Task update_with_no_saga_id_to_be_on_the_envelope()
        {
            await Exception<IndeterminateSagaStateIdException>.ShouldBeThrownByAsync(async () =>
            {
                await invoke(new CompleteFour());
            });

        }

        [Fact]
        public async Task update_with_no_saga_id_to_be_on_the_envelope_or_message()
        {
            await Exception<IndeterminateSagaStateIdException>.ShouldBeThrownByAsync(async () =>
            {
                await invoke(new IntCompleteThree());
            });

        }

        [Fact]
        public async Task straight_up_update_with_the_saga_id_on_the_message()
        {
            await send(new IntStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            var message = new IntCompleteThree
            {
                SagaId = stateId
            };

            await send(message);

            var state = LoadState(stateId);
            state.ThreeCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task update_with_message_that_uses_saga_identity_attributed_property()
        {
            await send(new IntStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            var message = new IntDoThree
            {
                TheSagaId = stateId
            };

            await send(message);

            var state = LoadState(stateId);
            state.ThreeCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task update_expecting_the_saga_id_to_be_on_the_envelope()
        {
            await send(new IntStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            await send(new CompleteFour(), stateId);

            var state = LoadState(stateId);
            state.FourCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task complete()
        {
            await send(new IntStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            await send(new FinishItAll(), stateId);

            LoadState(stateId).ShouldBeNull();
        }

        [Fact]
        public async Task handle_a_saga_message_with_cascading_messages_passes_along_the_saga_id_in_header()
        {
            await send(new IntStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            await send(new CompleteOne(), stateId);

            var state = LoadState(stateId);
            state.OneCompleted.ShouldBeTrue();
            state.TwoCompleted.ShouldBeTrue();

        }
    }
}
