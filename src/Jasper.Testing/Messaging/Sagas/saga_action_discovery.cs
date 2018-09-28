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
        private JasperRuntime _runtime;

        public async Task withRuntime()
        {
            if (_runtime == null)
            {
                _runtime = await JasperRuntime.BasicAsync();
            }
        }

        public HandlerChain ChainFor<T>()
        {
            return _runtime.Get<HandlerGraph>().ChainFor<T>();
        }

        public void Dispose()
        {
            _runtime?.Shutdown();
        }


    }

    public class saga_action_discovery : IClassFixture<SagaFixture>
    {
        private readonly SagaFixture _fixture;

        public saga_action_discovery(SagaFixture fixture)
        {
            _fixture = fixture;
        }

        private HandlerChain chainFor<T>()
        {
            return _fixture.ChainFor<T>();
        }

        [Fact]
        public async Task finds_actions_on_saga_state_handler_classes()
        {
            await _fixture.withRuntime();

            ShouldBeNullExtensions.ShouldNotBeNull(chainFor<SagaMessage2>());
        }

        [Fact]
        public async Task finds_actions_on_saga_state_orchestrates_methods()
        {
            await _fixture.withRuntime();
            ShouldBeNullExtensions.ShouldNotBeNull(chainFor<SagaMessage1>());
        }

        [Fact]
        public async Task finds_actions_on_saga_state_start_methods()
        {
            await _fixture.withRuntime();
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
        public Guid Id = Guid.NewGuid();

        public int FailThisManyTimes = 0;
    }
}
