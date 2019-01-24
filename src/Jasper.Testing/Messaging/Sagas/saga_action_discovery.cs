using System;
using System.Threading.Tasks;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Sagas
{
    public class SagaFixture : IDisposable
    {
        private IJasperHost _host;

        public void Dispose()
        {
            _host?.Dispose();
        }

        public void withRuntime()
        {
            if (_host == null) _host = JasperHost.Basic();
        }

        public HandlerChain ChainFor<T>()
        {
            return _host.Get<HandlerGraph>().ChainFor<T>();
        }
    }

    public class saga_action_discovery : IClassFixture<SagaFixture>
    {
        public saga_action_discovery(SagaFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly SagaFixture _fixture;

        private HandlerChain chainFor<T>()
        {
            return _fixture.ChainFor<T>();
        }

        [Fact]
        public void finds_actions_on_saga_state_handler_classes()
        {
            _fixture.withRuntime();

            ShouldBeNullExtensions.ShouldNotBeNull(chainFor<SagaMessage2>());
        }

        [Fact]
        public void finds_actions_on_saga_state_orchestrates_methods()
        {
            _fixture.withRuntime();
            ShouldBeNullExtensions.ShouldNotBeNull(chainFor<SagaMessage1>());
        }

        [Fact]
        public void finds_actions_on_saga_state_start_methods()
        {
            _fixture.withRuntime();
            ShouldBeNullExtensions.ShouldNotBeNull(chainFor<SagaStarter>());
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
