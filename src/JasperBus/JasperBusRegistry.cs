using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reflection;
using Baseline.Reflection;
using Jasper;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Routing;
using JasperBus.Runtime.Serializers;
using StructureMap.TypeRules;

namespace JasperBus
{
    public class JasperBusRegistry : JasperRegistry
    {
        private readonly ServiceBusFeature _feature;

        public JasperBusRegistry()
        {
            UseFeature<ServiceBusFeature>();

            _feature = Feature<ServiceBusFeature>();
            Serialization = new SerializationExpression(this);
        }

        public HandlerSource Handlers => _feature.Handlers;

        public Policies Policies => _feature.Policies;

        public SerializationExpression Serialization { get; }

        public ChannelExpression ListenForMessagesFrom(Uri uri)
        {
            var node = _feature.Channels[uri];
            node.Incoming = true;

            return new ChannelExpression(_feature.Channels, node);
        }

        public ChannelExpression ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(uriString.ToUri());
        }

        public SendExpression SendMessage<T>()
        {
            return new SendExpression(this, new SingleTypeRoutingRule<T>());
        }

        public SendExpression SendMessages(string description, Func<Type, bool> filter)
        {
            return new SendExpression(this, new LambdaRoutingRule(description, filter));
        }

        public SendExpression SendMessagesInNamespace(string @namespace)
        {
            return new SendExpression(this, new NamespaceRule(@namespace));
        }

        public SendExpression SendMessagesInNamespaceContaining<T>()
        {
            return SendMessagesInNamespace(typeof(T).Namespace);
        }

        public SendExpression SendMessagesFromAssembly(Assembly assembly)
        {
            return new SendExpression(this, new AssemblyRule(assembly));
        }

        public SendExpression SendMessagesFromAssemblyContaining<T>()
        {
            return SendMessagesFromAssembly(typeof(T).GetAssembly());
        }

        public class SendExpression
        {
            private readonly JasperBusRegistry _parent;
            private readonly IRoutingRule _routing;

            public SendExpression(JasperBusRegistry parent, IRoutingRule routing)
            {
                _parent = parent;
                _routing = routing;
            }

            public SendExpression To(Uri address)
            {
                _parent._feature.Channels[address].Rules.Add(_routing);
                return this;
            }

            public SendExpression To(string address)
            {
                return To(address.ToUri());
            }
        }

        public class SerializationExpression
        {
            private readonly JasperBusRegistry _parent;

            public SerializationExpression(JasperBusRegistry parent)
            {
                _parent = parent;
            }


            public SerializationExpression Add<T>() where T : IMessageSerializer
            {
                _parent._feature.Services.For<IMessageSerializer>().Add<T>();
                return this;
            }


            /// <summary>
            /// Specify or override the preferred order of serialization usage for the application
            /// </summary>
            /// <param name="contentTypes"></param>
            /// <returns></returns>
            public SerializationExpression ContentPreferenceOrder(params string[] contentTypes)
            {
                _parent._feature.Channels.AcceptedContentTypes.Clear();
                _parent._feature.Channels.AcceptedContentTypes.AddRange(contentTypes);
                return this;
            }
        }

        public ChannelExpression Channel(Uri uri)
        {
            var node = _feature.Channels[uri];
            return new ChannelExpression(_feature.Channels, node);
        }


        public ChannelExpression Channel(string uriString)
        {
            return Channel(uriString.ToUri());
        }
    }
}

