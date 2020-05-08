using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jasper.Pulsar
{
    public struct PulsarTopic
    {
        public string Persistence { get; }
        public string Tenant { get; }
        public string Namespace { get; }
        public string TopicName { get; }

        public bool IsForReply { get; }

        private const string PulsarTopicRegex = @"(non-persistent|persistent)://([-A-Za-z0-9]*)/([-A-Za-z0-9]*)/([-A-Za-z0-9]*)(/for-reply)?";

        private const string InvalidTopicFormatMessage =
            "Invalid Pulsar topic. Expecting format of \"{persistent|non-persistent}://tenant/namespace/topic\" (can append \"/for-reply\" for Jasper functionality. It will not be included in communication w/ Pulsar)";

        public PulsarTopic(Uri topic) : this(topic?.ToString())
        {

        }

        public PulsarTopic(string topic)
        {
            MatchCollection match = Regex.Matches(topic, PulsarTopicRegex, RegexOptions.Compiled);

            if (!match.Any())
                throw new ArgumentException(InvalidTopicFormatMessage, nameof(topic));

            Persistence = match[0].Groups[1].Captures[0].Value;
            Tenant = match[0].Groups[2].Captures[0].Value;
            Namespace = match[0].Groups[3].Captures[0].Value;
            TopicName = match[0].Groups[4].Captures[0].Value;
            IsForReply = match[0].Groups.Count == 6;
        }

        public static implicit operator string(PulsarTopic topic) => topic.ToString();
        public static implicit operator PulsarTopic(string topic) => new PulsarTopic(topic);
        public static implicit operator PulsarTopic(Uri topic) => new PulsarTopic(topic);

        public override string ToString() => $"{Persistence}://{Tenant}/{Namespace}/{TopicName}";

        public Uri ToJasperUri(bool forReply) => new Uri($"{this}{(forReply ? "/for-reply" : string.Empty)}");
    }
}
