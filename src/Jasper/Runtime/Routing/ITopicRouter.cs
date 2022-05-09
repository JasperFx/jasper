using System;
using Jasper.Transports;

namespace Jasper.Runtime.Routing;

public interface ITopicRouter : ISubscriber
{
    Uri BuildUriForTopic(string topicName);
}
