using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Jasper;
using Jasper.Consul;
using Jasper.Consul.Internal;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using Shouldly;
using Xunit;

namespace IntegrationTests.Consul
{
    [Collection("Consul")]
    public class SubscriptionComplianceSpecs :  IDisposable
    {
        private readonly IList<IDisposable> _disposables = new List<IDisposable>();
        private readonly JasperRuntime _coolColors1;
        private readonly JasperRuntime _coolColors2;
        private readonly JasperRuntime _warmColors;
        private readonly JasperRuntime _primaryColors;
        private readonly JasperRuntime _otherWarm;


        public SubscriptionComplianceSpecs()
        {
            using (var runtime = JasperRuntime.For(configure))
            {
                beforeEach(runtime);
            }

            _coolColors1 = app("Cool", _ =>
            {
                _.Handles<BlueMessage>();
                _.Handles<GreenMessage>();
                //_.Handles<PurpleMessage>();

                _.Publish.Message<CyanMessage>();

                _.Transports.LightweightListenerAt(2000);
            });

            _coolColors2 = app("Cool", _ =>
            {
                _.Handles<BlueMessage>();
                _.Handles<GreenMessage>();
                _.Handles<PurpleMessage>();

                _.Publish.Message<ChartreuseMessage>();

                _.Transports.LightweightListenerAt(2001);
            });

            _warmColors = app("Warm", _ =>
            {
                _.Handles<RedMessage>();
                _.Handles<YellowMessage>();
                _.Handles<OrangeMessage>();

                _.Publish.Message<MagentaMessage>();

                _.Transports.LightweightListenerAt(2002);
            });

            _primaryColors = app("Primary", _ =>
            {
                _.Publish.Message<BlueMessage>();
                _.Publish.Message<RedMessage>();
                _.Publish.Message<YellowMessage>();

                _.Transports.LightweightListenerAt(2003);
            });


            _otherWarm = app("OtherWarm", _ =>
            {
                _.Handles<RedMessage>();
            });
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }

        protected JasperRuntime app(string name, Action<ColorsApp> configureApp)
        {
            var registry = new ColorsApp(name);
            configureApp(registry);
            configure(registry);

            var runtime = JasperRuntime.For(registry);

            _disposables.Add(runtime);


            return runtime;
        }

        public void configure(JasperRegistry registry)
        {
            registry.Include<ConsulBackedSubscriptions>();
        }

        /// <summary>
        /// Clean off any existing state
        /// </summary>
        /// <param name="runtime"></param>
        protected void beforeEach(JasperRuntime runtime)
        {
            using (var client = new ConsulClient())
            {
                client.KV.DeleteTree(ConsulSubscriptionRepository.SUBSCRIPTION_PREFIX).Wait();
                client.KV.DeleteTree(ConsulSubscriptionRepository.CAPABILITY_PREFEX).Wait();
            }

        }



        [Fact]
        public virtual async Task persist_and_load_capabilities()
        {
            var repository = _coolColors1.Get<ISubscriptionsRepository>();
            await repository.PersistCapabilities(_coolColors1.Capabilities);

            var persisted = await repository.CapabilitiesFor(_coolColors1.ServiceName);

            persisted.ShouldNotBeSameAs(_coolColors1.Capabilities);

            persisted.Subscriptions.ShouldBe(_coolColors1.Capabilities.Subscriptions);
            persisted.Published.ShouldBe(_coolColors1.Capabilities.Published);
        }

        [Fact]
        public virtual async Task persist_replace_then_load_capabilities()
        {
            var repository = _coolColors1.Get<ISubscriptionsRepository>();
            await repository.PersistCapabilities(_coolColors1.Capabilities);
            await repository.PersistCapabilities(_coolColors2.Capabilities);

            var persisted = await repository.CapabilitiesFor(_coolColors2.ServiceName);

            persisted.ShouldNotBeSameAs(_coolColors2.Capabilities);

            persisted.Subscriptions.ShouldBe(_coolColors2.Capabilities.Subscriptions);
            persisted.Published.ShouldBe(_coolColors2.Capabilities.Published);
        }

        [Fact]
        public virtual async Task persist_and_remove_capabilities()
        {
            var repository = _coolColors1.Get<ISubscriptionsRepository>();
            await repository.PersistCapabilities(_coolColors1.Capabilities);
            await repository.PersistCapabilities(_warmColors.Capabilities);

            await repository.RemoveCapabilities(_coolColors1.ServiceName);

            (await repository.AllCapabilities()).Single().ServiceName
                .ShouldBe(_warmColors.ServiceName);
        }

        [Fact]
        public virtual async Task get_all_capabilities()
        {
            var repository = _coolColors1.Get<ISubscriptionsRepository>();
            await repository.PersistCapabilities(_coolColors1.Capabilities);
            await repository.PersistCapabilities(_warmColors.Capabilities);
            await repository.PersistCapabilities(_primaryColors.Capabilities);


            (await repository.AllCapabilities()).Select(x => x.ServiceName).OrderBy(x => x)
                .ShouldBe(new string[]{_coolColors1.ServiceName, _primaryColors.ServiceName, _warmColors.ServiceName});
        }


        [Fact]
        public virtual async Task get_all_subscriptions()
        {
            var repository = _coolColors1.Get<ISubscriptionsRepository>();
            await repository.PersistCapabilities(_coolColors1.Capabilities);
            await repository.PersistCapabilities(_warmColors.Capabilities);
            await repository.PersistCapabilities(_primaryColors.Capabilities);
            await repository.PersistCapabilities(_otherWarm.Capabilities);

            var all = _coolColors1.Capabilities.Subscriptions
                .Concat(_warmColors.Capabilities.Subscriptions)
                .Concat(_primaryColors.Capabilities.Subscriptions)
                .Concat(_otherWarm.Capabilities.Subscriptions);

            (await repository.GetSubscriptions()).ShouldBe(all);
        }

    }

    public class ColorsApp : JasperRegistry
    {
        public ColorsApp(string name)
        {
            ServiceName = name;
            Handlers.Discovery(x => x.DisableConventionalDiscovery());
        }

        public void Handles<T>()
        {
            Handlers.Discovery(x => x.IncludeType<Handler<T>>());
        }
    }

    public class PurpleMessage{}
    public class YellowMessage{}
    public class MagentaMessage{}
    public class CyanMessage{}
    public class ChartreuseMessage{}

    public class Handler<T>
    {
        public void Handle(T message)
        {

        }
    }
}
