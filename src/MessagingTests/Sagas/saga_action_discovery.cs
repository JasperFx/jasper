using System;
using Jasper;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace MessagingTests.Sagas
{

    public class saga_action_discovery : IntegrationContext
    {
        private DefaultApp _fixture;

        public saga_action_discovery(DefaultApp @default) : base(@default)
        {
            _fixture = @default;
        }

        private HandlerChain chainFor<T>()
        {
            return _fixture.ChainFor<T>();
        }

        [Fact]
        public void finds_actions_on_saga_state_handler_classes()
        {

            chainFor<SagaMessage2>().ShouldNotBeNull();
        }

        [Fact]
        public void finds_actions_on_saga_state_orchestrates_methods()
        {
            chainFor<SagaMessage1>().ShouldNotBeNull();
        }

        [Fact]
        public void finds_actions_on_saga_state_start_methods()
        {
            chainFor<SagaStarter>().ShouldNotBeNull();
        }
    }

    public class MySagaStateGuy : StatefulSagaOf<MySagaState>
    {
        public void Orchestrates(SagaMessage1 message)
        {
        }

        public void Handle(SagaMessage2 message)
        {
        }

        public MySagaState Start(SagaStarter starter)
        {
            return new MySagaState();
        }
    }

    public class SagaStarter : Message3
    {
    }

    public class SagaMessage1 : Message1
    {
    }

    public class SagaMessage2 : Message2
    {
    }

    public class MySagaState
    {
        public Guid Id { get; set; }
    }

    [MessageIdentity("Message1")]
    public class Message1
    {
        public Guid Id = Guid.NewGuid();
    }

    [MessageIdentity("Message2")]
    public class Message2
    {
        public Guid Id = Guid.NewGuid();
    }

    [MessageIdentity("Message3")]
    public class Message3
    {
    }

    [MessageIdentity("Message4")]
    public class Message4
    {
    }

    [MessageIdentity("Message5")]
    public class Message5
    {
        public int FailThisManyTimes = 0;
        public Guid Id = Guid.NewGuid();
    }
}
