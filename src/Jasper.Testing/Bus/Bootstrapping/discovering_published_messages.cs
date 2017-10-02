using Jasper.Bus;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class discovering_published_messages: BootstrappingContext
    {
        public discovering_published_messages()
        {
            theRegistry.Publish.Message(typeof(Message1));
            theRegistry.Publish.Message<Message2>();

            theRegistry.Publish.MessagesMatching(x => x.Name.StartsWith("Conventional"));
        }

        [Fact]
        public void has_explicitly_published_messages()
        {
            theRuntime.Capabilities.Publishes<Message1>().ShouldBeTrue();
            theRuntime.Capabilities.Publishes<Message2>().ShouldBeTrue();
        }

        [Fact]
        public void negative_case()
        {
            // doesn't match conventions and not explicitly added
            theRuntime.Capabilities.Publishes<Message3>().ShouldBeFalse();
        }

        [Fact]
        public void find_attributes()
        {
            theRuntime.Capabilities.Publishes<PublishedMessage1>().ShouldBeTrue();
            theRuntime.Capabilities.Publishes<PublishedMessage2>().ShouldBeTrue();
        }

        [Fact]
        public void find_by_conventions_against_the_application_assembly()
        {
            theRuntime.Capabilities.Publishes<ConventionalMessage1>().ShouldBeTrue();
            theRuntime.Capabilities.Publishes<ConventionalMessage2>().ShouldBeTrue();
            theRuntime.Capabilities.Publishes<ConventionalMessage3>().ShouldBeTrue();
        }
    }

    // SAMPLE: using-[Publish]
    [Publish]
    public class PublishedMessage1
    {

    }
    // ENDSAMPLE

    [Publish]
    public class PublishedMessage2
    {

    }


    public class ConventionalMessage1{}
    public class ConventionalMessage2{}
    public class ConventionalMessage3{}
}
