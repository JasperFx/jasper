using System;
using Baseline.ImTools;
using Baseline.Reflection;
using Jasper.Attributes;
using Jasper.Util;

namespace Jasper.Runtime.Routing;

internal static class TopicRouting
{
    private static ImHashMap<Type, string> _topics = ImHashMap<Type, string>.Empty;
    
    public static string DetermineTopicName(Type messageType)
    {
        if (_topics.TryFind(messageType, out var topic))
        {
            return topic;
        }

        topic = messageType.HasAttribute<TopicAttribute>()
            ? messageType.GetAttribute<TopicAttribute>()!.TopicName
            : messageType.ToMessageTypeName();

        _topics = _topics.AddOrUpdate(messageType, topic);

        return topic;
    }
}
