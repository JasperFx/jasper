using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Runtime.Subscriptions.New;
using StoryTeller;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    public class CapabilitiesFixture : BusFixture
    {
        private ServiceCapabilities _current;
        private readonly IList<ServiceCapabilities> _services = new List<ServiceCapabilities>();

        public CapabilitiesFixture()
        {
            Title = "Messaging and Subscription Fixture";
        }

        public override void SetUp()
        {
            _services.Clear();
        }

        public IGrammar ForService()
        {
            return Embed<ServiceCapabilityFixture>("If a service has capabilities:")
                .After(c =>
                {
                    _current = Context.State.Retrieve<ServiceCapabilities>();
                    _services.Add(_current);
                });
        }

        [FormatAs("No capability errors were found")]
        public bool NoErrorsWereFound()
        {
            StoryTellerAssert.Fail(_current.Errors.Any(), () => $@"Found:
{_current.Errors.Select(x => $"* {x}{Environment.NewLine}")}
");
            return true;
        }

        public IGrammar TheErrorsDetectedWere()
        {
            return VerifyStringList(() => _current.Errors)
                .Titled("The detected capability errors were");
        }

        public IGrammar ThePublishedMessagesAre()
        {
            return VerifySetOf<PublishedMessage>(() => _current.Published)
                .Titled("The published messages should be")
                .MatchOn(x => x.MessageType, x => x.ContentTypes);
        }

        public IGrammar TheSubscriptionsAre()
        {
            return VerifySetOf<Subscription>(() => _current.Subscriptions)
                .Titled("The required subscriptions should be")
                .MatchOn(x => x.MessageType, x => x.Destination, x => x.Accept);
        }

        public IGrammar ValidationShouldBe()
        {
            return Embed<MessagingGraphFixture>("The messaging capability across services should be")
                .Before(c => c.State.Store(new MessagingGraph(_services.ToArray())));
        }
    }
}
