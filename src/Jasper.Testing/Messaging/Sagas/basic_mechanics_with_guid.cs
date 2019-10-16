﻿using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Sagas;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Sagas
{
    [JasperIgnore]
    public class GuidBasicWorkflow : BasicWorkflow<GuidWorkflowState, GuidStart, GuidCompleteThree, Guid>
    {
        public GuidWorkflowState Starts(WildcardStart start)
        {
            var sagaId = Guid.Parse(start.Id);
            return new GuidWorkflowState
            {
                Id = sagaId,
                Name = start.Name
            };
        }

        public void Handles(GuidDoThree message, GuidWorkflowState state)
        {
            state.ThreeCompleted = true;
        }
    }

    public class GuidDoThree
    {
        [SagaIdentity] public Guid TheSagaId { get; set; }
    }

    public class basic_mechanics_with_guid : SagaTestHarness<GuidBasicWorkflow, GuidWorkflowState>
    {
        private readonly Guid stateId = Guid.NewGuid();

        [Fact]
        public async Task complete()
        {
            await send(new GuidStart
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
            await send(new GuidStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            await send(new CompleteOne(), stateId);

            var state = LoadState(stateId);
            state.OneCompleted.ShouldBeTrue();
            state.TwoCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task start_1()
        {
            await send(new GuidStart
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
        public async Task straight_up_update_with_the_saga_id_on_the_message()
        {
            await send(new GuidStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            var message = new GuidCompleteThree
            {
                SagaId = stateId
            };

            await send(message);

            var state = LoadState(stateId);
            state.ThreeCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task update_expecting_the_saga_id_to_be_on_the_envelope()
        {
            await send(new GuidStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            await send(new CompleteFour(), stateId);

            var state = LoadState(stateId);
            state.FourCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task update_with_message_that_uses_saga_identity_attributed_property()
        {
            await send(new GuidStart
            {
                Id = stateId,
                Name = "Croaker"
            });

            var message = new GuidDoThree
            {
                TheSagaId = stateId
            };

            await send(message);

            var state = LoadState(stateId);
            state.ThreeCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task update_with_no_saga_id_to_be_on_the_envelope()
        {
            await Should.ThrowAsync<IndeterminateSagaStateIdException>(async () =>
            {
                await invoke(new CompleteFour());
            });
        }

        [Fact]
        public async Task update_with_no_saga_id_to_be_on_the_envelope_or_message()
        {
            await Should.ThrowAsync<IndeterminateSagaStateIdException>(async () =>
            {
                await invoke(new GuidCompleteThree());
            });
        }
    }
}
