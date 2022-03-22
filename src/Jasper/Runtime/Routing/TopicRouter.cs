using System;
using System.Collections.Generic;
using System.Linq;
using Baseline.ImTools;
using Baseline.Reflection;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Runtime.Routing
{
    public abstract class TopicRouter<TSubscriberConfiguration> : Subscriber, ITopicRouter
    {
        private static ImHashMap<Type, string> _topics = ImHashMap<Type, string>.Empty;

        public readonly IList<ITopicRule> TopicRules = new List<ITopicRule>();

        public abstract Uri? BuildUriForTopic(string? topicName);

        public static string? DetermineTopicName(Type messageType)
        {
            if (_topics.TryFind(messageType, out var topic)) return topic;

            topic = messageType.HasAttribute<TopicAttribute>()
                ? messageType.GetAttribute<TopicAttribute>().TopicName
                : messageType.ToMessageTypeName();

            _topics = _topics.AddOrUpdate(messageType, topic);

            return topic;
        }

        public abstract TSubscriberConfiguration FindConfigurationForTopic(string topicName, IEndpoints endpoints);


        public override void AddRoute(MessageTypeRouting routing, IJasperRuntime root)
        {
            var messageType = routing.MessageType;
            var rule = TopicRules.FirstOrDefault(x => x.Matches(messageType));

            if (rule == null)
            {
                var topicName = DetermineTopicName(messageType);
                var uri = BuildUriForTopic(topicName);
                var agent = root.Runtime.GetOrBuildSendingAgent(uri);
                agent.Endpoint.AddRoute(routing, root);
            }
            else
            {
                routing.AddTopicRoute(rule, this);
            }
        }

    }
}
