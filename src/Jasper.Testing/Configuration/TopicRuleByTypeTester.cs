using Jasper.Configuration;
using Jasper.Runtime.Routing;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class TopicRuleByTypeTester
    {
        [Fact]
        public void miss()
        {
            var rule = new TopicRuleByType<Message1>(m => "wrong");

            rule.Matches(typeof(LogMessage))
                .ShouldBeFalse();

        }

        [Fact]
        public void hit()
        {
            var rule = new TopicRuleByType<LogMessage>(m => m.Priority);

            rule.Matches(typeof(LogMessage))
                .ShouldBeTrue();

            rule.DetermineTopicName(new LogMessage{Priority = "Low"})
                .ShouldBe("Low");
        }
    }

    public class LogMessage
    {
        public string Priority { get; set; }
    }
}
