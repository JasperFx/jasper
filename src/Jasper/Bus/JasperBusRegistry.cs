using System;
using System.Reflection;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Conneg;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.TypeRules;

namespace Jasper.Bus
{
    public class MessagesExpression
    {
        private readonly ServiceBusFeature _bus;

        public MessagesExpression(ServiceBusFeature bus)
        {
            _bus = bus;
        }

        public SendExpression SendMessage<T>()
        {
            return new SendExpression(_bus, new SingleTypeRoutingRule<T>());
        }

        public SendExpression SendMessages(string description, Func<Type, bool> filter)
        {
            return new SendExpression(_bus, new LambdaRoutingRule(description, filter));
        }

        public SendExpression SendMessagesInNamespace(string @namespace)
        {
            return new SendExpression(_bus, new NamespaceRule(@namespace));
        }

        public SendExpression SendMessagesInNamespaceContaining<T>()
        {
            return SendMessagesInNamespace(typeof(T).Namespace);
        }

        public SendExpression SendMessagesFromAssembly(Assembly assembly)
        {
            return new SendExpression(_bus, new AssemblyRule(assembly));
        }

        public SendExpression SendMessagesFromAssemblyContaining<T>()
        {
            return SendMessagesFromAssembly(typeof(T).GetAssembly());
        }

        public class SendExpression
        {
            private readonly ServiceBusFeature _bus;
            private readonly IRoutingRule _routing;

            public SendExpression(ServiceBusFeature bus, IRoutingRule routing)
            {
                _bus = bus;
                _routing = routing;
            }

            public SendExpression To(Uri address)
            {
                _bus.Channels[address].Rules.Add(_routing);
                return this;
            }

            public SendExpression To(string address)
            {
                return To(address.ToUri());
            }
        }
    }


    public class JasperBusRegistry : JasperRegistry
    {
        public JasperBusRegistry()
        {
            Messages = new MessagesExpression(_bus);
        }

        public MessagesExpression Messages { get; }


        public class SerializationExpression
        {
            private readonly ServiceBusFeature _parent;



            public SerializationExpression(ServiceBusFeature parent)
            {
                _parent = parent;
            }


            public SerializationExpression Add<T>() where T : ISerializer
            {
                _parent.Services.For<ISerializer>().Add<T>();
                return this;
            }


            /// <summary>
            /// Specify or override the preferred order of serialization usage for the application
            /// </summary>
            /// <param name="contentTypes"></param>
            /// <returns></returns>
            public SerializationExpression ContentPreferenceOrder(params string[] contentTypes)
            {
                _parent.Channels.AcceptedContentTypes.Clear();
                _parent.Channels.AcceptedContentTypes.AddRange(contentTypes);
                return this;
            }
        }

        public class SubscriptionExpression
        {
            private readonly ServiceBusFeature _parent;
            private readonly Uri _receiving;

            public SubscriptionExpression(ServiceBusFeature parent, Uri receiving)
            {
                _parent = parent;
                _receiving = receiving;

                parent.Services.AddType(typeof(ISubscriptionRequirements), typeof(SubscriptionRequirements));
            }

            /// <summary>
            /// Specify the publishing source of the events you want to subscribe to
            /// </summary>
            /// <param name="sourceProperty"></param>
            /// <returns></returns>
            public TypeSubscriptionExpression ToSource(string sourceProperty)
            {
                return ToSource(sourceProperty.ToUri());
            }

            /// <summary>
            /// Specify the publishing source of the events you want to subscribe to
            /// </summary>
            /// <param name="sourceProperty"></param>
            /// <returns></returns>
            public TypeSubscriptionExpression ToSource(Uri sourceProperty)
            {
                var requirement = _receiving == null
                    ? (ISubscriptionRequirement)new LocalSubscriptionRequirement(sourceProperty)
                    : new GroupSubscriptionRequirement(sourceProperty, _receiving);

                _parent.Services.AddService(requirement);

                return new TypeSubscriptionExpression(requirement);
            }

            public class TypeSubscriptionExpression
            {
                private readonly ISubscriptionRequirement _requirement;

                public TypeSubscriptionExpression(ISubscriptionRequirement requirement)
                {
                    _requirement = requirement;
                }

                public TypeSubscriptionExpression ToMessage<TMessage>()
                {
                    _requirement.AddType(typeof(TMessage));

                    return this;
                }

                public TypeSubscriptionExpression ToMessage(Type messageType)
                {
                    _requirement.AddType(messageType);
                    return this;
                }
            }
        }

        public void OnMissingHandler<T>() where T : IMissingHandler
        {
            Services.AddService<IMissingHandler, T>();
        }

        public void OnMissingHandler(IMissingHandler handler)
        {
            Services.For<IMissingHandler>().Add(handler);
        }

        public SubscriptionExpression SubscribeAt(string receiving)
        {
            return SubscribeAt(receiving.ToUri());
        }

        public SubscriptionExpression SubscribeAt(Uri receiving)
        {
            return new SubscriptionExpression(_bus, receiving);
        }

        public SubscriptionExpression SubscribeLocally()
        {
            return new SubscriptionExpression(_bus, null);
        }


        public DelayedJobExpression DelayedJobs => new DelayedJobExpression(_bus);

        public class DelayedJobExpression
        {
            private readonly ServiceBusFeature _feature;

            public DelayedJobExpression(ServiceBusFeature feature)
            {
                _feature = feature;
            }

            public void RunInMemory()
            {
                _feature.DelayedJobsRunInMemory = true;
            }

            public void Use<T>() where T : class, IDelayedJobProcessor
            {
                _feature.DelayedJobsRunInMemory = false;
                _feature.Services.ForSingletonOf<IDelayedJobProcessor>().Use<T>();
            }

            public void Use(IDelayedJobProcessor delayedJobs)
            {
                _feature.DelayedJobsRunInMemory = false;
                _feature.Services.ForSingletonOf<IDelayedJobProcessor>().Use(delayedJobs);
            }
        }
    }
}

