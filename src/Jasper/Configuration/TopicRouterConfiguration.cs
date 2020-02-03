using System;
using Baseline;
using Jasper.Runtime.Routing;

namespace Jasper.Configuration
{
    public class TopicRouterConfiguration<TSubscriberConfiguration>
    {
        private readonly TopicRouter<TSubscriberConfiguration> _router;
        private readonly IEndpoints _endpoints;

        public TopicRouterConfiguration(TopicRouter<TSubscriberConfiguration> router, IEndpoints endpoints)
        {
            _router = router;
            _endpoints = endpoints;
        }

        /// <summary>
        ///     Force any messages enqueued to this worker queue to be durable
        /// </summary>
        /// <returns></returns>
        public TopicRouterConfiguration<TSubscriberConfiguration>  Durably()
        {
            _router.IsDurable = true;
            return this;
        }

        /// <summary>
        /// By default, messages on this worker queue will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        public TopicRouterConfiguration<TSubscriberConfiguration>  Lightweight()
        {
            _router.IsDurable = false;
            return this;
        }

        /// <summary>
        /// Determine a topic name for messages of type TMessage. This
        /// is meant for more complex topic routing where you may be using
        /// information about the message itself to decide how to route
        /// </summary>
        /// <param name="topicSource"></param>
        /// <typeparam name="TMessage"></typeparam>
        /// <returns></returns>
        public TopicRouterConfiguration<TSubscriberConfiguration>  OutgoingTopicNameIs<TMessage>(Func<TMessage, string> topicSource)
        {
            var rule = new TopicRuleByType<TMessage>(topicSource);
            return AddOutgoingTopicRule(rule);
        }

        /// <summary>
        /// Add a topic determination rule to this endpoint
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public TopicRouterConfiguration<TSubscriberConfiguration>  AddOutgoingTopicRule(ITopicRule rule)
        {
            _router.TopicRules.Add(rule);

            return this;
        }

        /// <summary>
        /// Add a topic rule to this endpoint by rule type
        /// </summary>
        /// <typeparam name="TRule"></typeparam>
        /// <returns></returns>
        public TopicRouterConfiguration<TSubscriberConfiguration>  AddOutgoingTopicRule<TRule>() where TRule : ITopicRule, new()
        {
            return AddOutgoingTopicRule(new TRule());
        }

        /// <summary>
        /// Configure the outgoing message sending for one specific topic
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
}
