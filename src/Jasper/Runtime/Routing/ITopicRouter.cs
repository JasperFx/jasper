using System;

namespace Jasper.Runtime.Routing
{
    public interface ITopicRouter
    {
        Uri? BuildUriForTopic(string? topicName);
    }
}
