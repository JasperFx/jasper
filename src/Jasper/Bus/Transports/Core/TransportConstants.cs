using System;
using Jasper.Util;

namespace Jasper.Bus.Transports.Core
{
    public static class TransportConstants
    {
        public static readonly string Default = "default";
        public static readonly string Replies = "replies";

        public static readonly Uri RetryUri = "loopback://retries".ToUri();
        public static readonly Uri RepliesUri = "loopback://replies".ToUri();
        public static readonly Uri DelayedUri = "loopback://delayed".ToUri();

    }
}
