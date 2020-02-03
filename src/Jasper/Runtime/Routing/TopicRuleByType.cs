using System;
using Baseline;

namespace Jasper.Runtime.Routing
{
    public class TopicRuleByType<T> : ITopicRule
    {
        private readonly Func<T, string> _source;

        public TopicRuleByType(Func<T, string> source)
        {
            _source = source;
        }

        public bool Matches(Type messageType)
        {
            return messageType.CanBeCastTo<T>();
        }

        public string DetermineTopicName(object message)
        {
            return _source((T) message);
        }

    }
}
