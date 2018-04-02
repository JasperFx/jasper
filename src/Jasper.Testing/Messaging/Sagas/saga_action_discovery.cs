using System;
using System.Threading.Tasks;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Jasper.Testing.Messaging.Runtime;
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
            _runtime?.Dispose();
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

            chainFor<SagaMessage2>().ShouldNotBeNull();
        }

        [Fact]
        public async Task finds_actions_on_saga_state_orchestrates_methods()
        {
            await _fixture.withRuntime();
            chainFor<SagaMessage1>().ShouldNotBeNull();
        }

        [Fact]
        public async Task finds_actions_on_saga_state_start_methods()
        {
            await _fixture.withRuntime();
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
}
