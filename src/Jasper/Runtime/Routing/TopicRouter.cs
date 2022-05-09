using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Configuration;

namespace Jasper.Runtime.Routing;

public abstract class TopicRouter<TSubscriberConfiguration> : Subscriber, ITopicRouter
{
    public readonly IList<ITopicRule> TopicRules = new List<ITopicRule>();

    public abstract Uri BuildUriForTopic(string topicName);


    public override void AddRoute(MessageTypeRouting routing, IJasperRuntime runtime)
    {
        var messageType = routing.MessageType;
        var rule = TopicRules.FirstOrDefault(x => x.Matches(messageType));

        if (rule == null)
        {
            var topicName = TopicRouting.DetermineTopicName(messageType);
            var uri = BuildUriForTopic(topicName);
            var agent = runtime.Endpoints.GetOrBuildSendingAgent(uri);
            agent.Endpoint.AddRoute(routing, runtime);
        }
        else
        {
            routing.AddTopicRoute(rule, this);
        }
    }



    public abstract TSubscriberConfiguration FindConfigurationForTopic(string topicName, IEndpoints endpoints);
}
