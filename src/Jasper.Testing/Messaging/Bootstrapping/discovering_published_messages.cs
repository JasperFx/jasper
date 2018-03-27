using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Testing.Messaging.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
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
        public async Task has_explicitly_published_messages()
        {
            var capabilities = (await theRuntime()).Capabilities;

            capabilities.Publishes<Message1>().ShouldBeTrue();
            capabilities.Publishes<Message2>().ShouldBeTrue();
        }

        [Fact]
        public async Task negative_case()
        {
            var capabilities = (await theRuntime()).Capabilities;

            // doesn't match conventions and not explicitly added
            capabilities.Publishes<Message3>().ShouldBeFalse();
        }

        [Fact]
        public async Task find_attributes()
        {
            var capabilities = (await theRuntime()).Capabilities;

            capabilities.Publishes<PublishedMessage1>().ShouldBeTrue();
            capabilities.Publishes<PublishedMessage2>().ShouldBeTrue();
        }

        [Fact]
        public async Task find_by_conventions_against_the_application_assembly()
        {
            var capabilities = (await theRuntime()).Capabilities;

            capabilities.Publishes<ConventionalMessage1>().ShouldBeTrue();
            capabilities.Publishes<ConventionalMessage2>().ShouldBeTrue();
            capabilities.Publishes<ConventionalMessage3>().ShouldBeTrue();
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
