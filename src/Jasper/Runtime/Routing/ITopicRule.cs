using System;

namespace Jasper.Runtime.Routing;

public interface ITopicRule
{
    bool Matches(Type messageType);
    string DetermineTopicName(object message);
}

