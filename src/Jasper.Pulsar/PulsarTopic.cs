using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jasper.Pulsar
{
    public static class PulsarPersistence
    {
        public const string Persistent = "persistent";
        public const string NonPersistent = "non-persistent";
    }

    public struct PulsarTopic
    {
        public string Persistence { get; }
        public string Tenant { get; }
        public string Namespace { get; }
        public string TopicName { get; }

        private const string PulsarTopicRegex = @"(non-persistent|persistent)://([-A-Za-z0-9]*)/([-A-Za-z0-9]*)/([-A-Za-z0-9]*)?";

        private const string InvalidTopicFormatMessage =
            "Invalid Pulsar topic. Expecting format of \"{persistent|non-persistent}://tenant/namespace/topic\"";

        public PulsarTopic(Uri topic) : this(topic?.ToString())
        {

        }

        public PulsarTopic(string fullyQualifiedTopic)
        {
            MatchCollection match = Regex.Matches(fullyQualifiedTopic, PulsarTopicRegex, RegexOptions.Compiled);

            if (!match.Any())
                throw new ArgumentException(InvalidTopicFormatMessage, nameof(fullyQualifiedTopic));

            Persistence = match[0].Groups[1].Captures[0].Value;
            Tenant = match[0].Groups[2].Captures[0].Value;
            Namespace = match[0].Groups[3].Captures[0].Value;
            TopicName = match[0].Groups[4].Captures[0].Value;
        }

        public static explicit operator string(PulsarTopic topic) => topic.ToString();
        public static implicit operator Uri(PulsarTopic topic) => new Uri(topic.ToString());
        public static implicit operator PulsarTopic(string topic) => new PulsarTopic(topic);
        public static implicit operator PulsarTopic(Uri topic) => new PulsarTopic(topic);

        public override string ToString() => $"{Persistence}://{Tenant}/{Namespace}/{TopicName}"; }
}
