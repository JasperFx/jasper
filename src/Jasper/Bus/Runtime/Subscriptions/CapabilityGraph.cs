using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Bus.Transports;
using Jasper.Conneg;
using Jasper.Util;
using TypeClassification = BlueMilk.Scanning.TypeClassification;
using TypeRepository = BlueMilk.Scanning.TypeRepository;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class CapabilityGraph : ISubscriptions
    {
        private readonly IList<PublishedMessage> _published = new List<PublishedMessage>();
        private readonly IList<SubscriptionRequirement> _requirements = new List<SubscriptionRequirement>();
        private readonly IList<Func<Type, bool>> _publishFilters = new List<Func<Type, bool>>();

        public CapabilityGraph()
        {
            _publishFilters.Add(type => type.HasAttribute<PublishAttribute>());
        }

        public Uri DefaultReceiverLocation { get; set; }

        public void Publish(Type messageType)
        {
            _published.Add(new PublishedMessage(messageType));
        }

        public async Task<ServiceCapabilities> Compile(HandlerGraph handlers, SerializationGraph serialization, IChannelGraph channels, JasperRuntime runtime, ITransport[] transports, UriAliasLookup lookups)
        {
            if (runtime.ApplicationAssembly != null)
            {
                var publishedTypes = await TypeRepository.FindTypes(runtime.ApplicationAssembly,
                    TypeClassification.Concretes | TypeClassification.Closed, type => _publishFilters.Any(x => x(type)));

                foreach (var type in publishedTypes)
                {
                    Publish(type);
                }
            }



            var capabilities = await compile(handlers, serialization, channels, transports, lookups, runtime);


            capabilities.ServiceName = runtime.ServiceName;

            return capabilities;
        }



        private async Task<ServiceCapabilities> compile(HandlerGraph handlers, SerializationGraph serialization, IChannelGraph channels, ITransport[] transports, UriAliasLookup lookups, JasperRuntime runtime)
        {
            var validTransports = transports.Select(x => x.Protocol).ToArray();

            var capabilities = new ServiceCapabilities
            {
                ServiceName = runtime.ServiceName,
                Subscriptions = determineSubscriptions(handlers, serialization),
                Published = determinePublishedMessages(serialization, runtime, validTransports)
            };

            // Hokey.
            foreach (var subscription in capabilities.Subscriptions)
            {
                subscription.ServiceName = runtime.ServiceName;
            }

            foreach (var message in capabilities.Published)
            {
                message.ServiceName = runtime.ServiceName;
            }

            await capabilities.ApplyLookups(lookups);

            // Now, do some validation
            var missingDestination = capabilities.Subscriptions
                .Where(x => x.Destination == null)
                .Select(s => $"Could not determine an incoming receiver for message '{s.MessageType}'");


            var invalidTransport = capabilities.Subscriptions
                .Where(x => x.Destination != null && !validTransports.Contains(x.Destination.Scheme))
                .Select(x => $"Unknown transport '{x.Destination.Scheme}' for subscription to message '{x.MessageType}'");

            var missingHandlers = capabilities.Subscriptions
                .Where(x => !handlers.CanHandle(x.DotNetType))
                .Select(x => $"No handler for message '{x.MessageType}' referenced in a subscription");

            capabilities.Errors = missingDestination.Concat(invalidTransport).Concat(missingHandlers).ToArray();


            return capabilities;
        }

        private PublishedMessage[] determinePublishedMessages(SerializationGraph serialization, JasperRuntime runtime, string[] validTransports)
        {
            foreach (var published in _published)
            {
                published.ServiceName = runtime.ServiceName;
                var writer = serialization.WriterFor(published.DotNetType);
                published.ContentTypes = writer.ContentTypes;
                published.Transports = validTransports;
            }

            return _published.ToArray();
        }

        private Subscription[] determineSubscriptions(HandlerGraph handlers, SerializationGraph serialization)
        {
            var messageTypes = handlers.Chains
                .Select(x => x.MessageType)
                .Where(t => t.GetTypeInfo().Assembly != GetType().GetTypeInfo().Assembly)
                .ToArray();


            return _requirements.SelectMany(x =>
                    x.DetermineSubscriptions(serialization, messageTypes, DefaultReceiverLocation))
                .Distinct()
                .ToArray();
        }

        void ISubscriptionReceiverExpression.At(Uri incoming)
        {
            DefaultReceiverLocation = incoming;
        }

        void ISubscriptionReceiverExpression.At(string incomingUriString)
        {
            DefaultReceiverLocation = incomingUriString.ToUri();
        }

        private ISubscriptionExpression add(Action<ISubscriptionExpression> configure)
        {
            var requirement = new SubscriptionRequirement();
            configure(requirement);
            _requirements.Add(requirement);

            return requirement;
        }

        ISubscriptionExpression ISubscriptionExpression.To<T>()
        {
            return add(x => x.To<T>());
        }

        ISubscriptionExpression ISubscriptionExpression.To(Type messageType)
        {
            return add(x => x.To(messageType));
        }

        ISubscriptionExpression ISubscriptionExpression.To(Func<Type, bool> filter)
        {
            return add(x => x.To(filter));
        }

        ISubscriptionReceiverExpression ISubscriptions.ToAllMessages()
        {
            return add(x => x.To(t => true));
        }

        internal void PublishesMessagesMatching(Func<Type, bool> filter)
        {
            _publishFilters.Add(filter);
        }

    }
}
