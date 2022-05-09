using System;
using Jasper.Runtime.Routing;

namespace Jasper.Configuration;

public class TopicRouterConfiguration<TSubscriberConfiguration>
{
    private readonly IEndpoints _endpoints;
    private readonly TopicRouter<TSubscriberConfiguration> _router;

    public TopicRouterConfiguration(TopicRouter<TSubscriberConfiguration> router, IEndpoints endpoints)
    {
        _router = router;
        _endpoints = endpoints;
    }

    /// <summary>
    ///     Force any messages enqueued to this worker queue to be durable so that outgoing
    ///     messages are persisted to durable storage until successfully sent. This is necessary
    ///     to take advantage of the outbox functionality
    /// </summary>
    /// <returns></returns>
    public TopicRouterConfiguration<TSubscriberConfiguration> DurablyStoreAndForward()
    {
        _router.Mode = EndpointMode.Durable;
        return this;
    }

    /// <summary>
    ///     By default, outgoing messages to this topic are queued in memory
    ///     with retry mechanics
    /// </summary>
    /// <returns></returns>
    public TopicRouterConfiguration<TSubscriberConfiguration> BufferedInMemory()
    {
        _router.Mode = EndpointMode.BufferedInMemory;
        return this;
    }

    /// <summary>
    ///     By default, outgoing messages to this topic are sent inline to the outgoing
    ///     sender in a predictable way with no retries
    /// </summary>
    /// <returns></returns>
    public TopicRouterConfiguration<TSubscriberConfiguration> SendInline()
    {
        _router.Mode = EndpointMode.Inline;
        return this;
    }

    /// <summary>
    ///     Determine a topic name for messages of type TMessage. This
    ///     is meant for more complex topic routing where you may be using
    ///     information about the message itself to decide how to route
    /// </summary>
    /// <param name="topicSource"></param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public TopicRouterConfiguration<TSubscriberConfiguration> OutgoingTopicNameIs<TMessage>(
        Func<TMessage, string> topicSource)
    {
        var rule = new TopicRuleByType<TMessage>(topicSource);
        return AddOutgoingTopicRule(rule);
    }

    /// <summary>
    ///     Add a topic determination rule to this endpoint
    /// </summary>
    /// <param name="rule"></param>
    /// <returns></returns>
    public TopicRouterConfiguration<TSubscriberConfiguration> AddOutgoingTopicRule(ITopicRule rule)
    {
        _router.TopicRules.Add(rule);

        return this;
    }

    /// <summary>
    ///     Add a topic rule to this endpoint by rule type
    /// </summary>
    /// <typeparam name="TRule"></typeparam>
    /// <returns></returns>
    public TopicRouterConfiguration<TSubscriberConfiguration> AddOutgoingTopicRule<TRule>()
        where TRule : ITopicRule, new()
    {
        return AddOutgoingTopicRule(new TRule());
    }

    /// <summary>
    ///     Configure the outgoing message sending for one specific topic
    /// </summary>
    /// <param name="topicName"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public TopicRouterConfiguration<TSubscriberConfiguration> ConfigureTopicConfiguration(string topicName,
        Action<TSubscriberConfiguration> configure)
    {
        var configuration = _router.FindConfigurationForTopic(topicName, _endpoints);
        configure(configuration);

        return this;
    }
}
