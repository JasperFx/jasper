using System;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Runtime.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{

    #region sample_using_Topic_attribute
    [Topic("one")]
    public class TopicMessage1
    {

    }
    #endregion


    public class ColorMessagee
    {
        public string Color { get; set; }
    }

    [MessageIdentity("one")]
    public class M1{}

    [Topic("two")]
    public class M2{}

    [Topic("three")]
    [MessageIdentity("third")]
    public class M3{}




    public class TopicRouterTester
    {
        [Theory]
        [InlineData(typeof(M1), "one")]
        [InlineData(typeof(M2), "two")]
        [InlineData(typeof(M3), "three")]
        public void determine_topic_name_by_type(Type messageType, string expected)
        {
            // Do it repeatedly just to hammer on the memoization a bit
            TopicRouter<string>.DetermineTopicName(messageType).ShouldBe(expected);
            TopicRouter<string>.DetermineTopicName(messageType).ShouldBe(expected);
            TopicRouter<string>.DetermineTopicName(messageType).ShouldBe(expected);
        }

    }
}
