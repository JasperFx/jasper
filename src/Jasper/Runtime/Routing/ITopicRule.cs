using System;

namespace Jasper.Runtime.Routing
{
    public interface ITopicRule
    {
        bool Matches(Type messageType);
        string? DetermineTopicName(object? message);
    }

    public class ConstantTopicRule : ITopicRule
    {
        private readonly string? _topicName;

        public ConstantTopicRule(string? topicName)
        {
            _topicName = topicName;
        }

        public bool Matches(Type messageType)
        {
            return true;
        }

        public string? DetermineTopicName(object? message)
        {
            return _topicName;
        }
    }
}
