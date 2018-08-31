using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Runtime.Subscriptions;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    public class SubscriptionDeltaFixture : Fixture
    {
        private readonly IList<Subscription> _actual = new List<Subscription>();
        private readonly IList<Subscription> _expected = new List<Subscription>();
        private SubscriptionDelta _delta;

        public IGrammar TheExistingAre()
        {
            return CreateNewObject<Subscription>(_ =>
                {
                    _.SetProperty(x => x.MessageType);
                    _.SetProperty(x => x.Destination);
                    _.SetProperty(x => x.Accept);
                    _.Do(x => _actual.Add(x));
                }).AsTable("The existing subscriptions in storage are")
                .Before(() => _actual.Clear());
        }

        public IGrammar TheExpectedAre()
        {
            return CreateNewObject<Subscription>(_ =>
                {
                    _.SetProperty(x => x.MessageType);
                    _.SetProperty(x => x.Destination);
                    _.SetProperty(x => x.Accept);
                    _.Do(x => _expected.Add(x));
                }).AsTable("The expected subscriptions are")
                .Before(() => _expected.Clear())
                .After(() => _delta = new SubscriptionDelta(_expected.ToArray(), _actual.ToArray()));
        }

        public IGrammar ToBeCreated()
        {
            return VerifySetOf(() => _delta.NewSubscriptions)
                .Titled("The missing subscriptions to be created are")
                .MatchOn(x => x.MessageType, x => x.Destination, x => x.Accept);
        }

        public IGrammar ToBeDeleted()
        {
            return VerifySetOf(() => _delta.ObsoleteSubscriptions)
                .Titled("The obsolete subscriptions to be deleted are")
                .MatchOn(x => x.MessageType, x => x.Destination, x => x.Accept);
        }
    }
}
