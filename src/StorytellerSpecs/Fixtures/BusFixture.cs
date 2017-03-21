using System;
using System.Linq;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{
    public abstract class BusFixture : Fixture
    {
        public static Uri Channel1 = new Uri("stub://one");
        public static Uri Channel2 = new Uri("stub://two");
        public static Uri Channel3 = new Uri("stub://three");
        public static Uri Channel4 = new Uri("stub://four");

        public static Uri LQChannel1 = new Uri("lq.tcp://localhost:2201/one");
        public static Uri LQChannel2 = new Uri("lq.tcp://localhost:2201/two");
        public static Uri LQChannel3 = new Uri("lq.tcp://localhost:2201/three");
        public static Uri LQChannel4 = new Uri("lq.tcp://localhost:2201/four");

        protected readonly Type[] messageTypes =
        {
            typeof(Message1), typeof(Message2), typeof(Message3), typeof(Message4),
            typeof(Message5), typeof(Message6), typeof(ErrorMessage)
        };


        protected BusFixture()
        {
            AddSelectionValues("MessageTypes", messageTypes.Select(x => x.Name).ToArray());
            AddSelectionValues("Channels", "stub://one", "stub://two", "stub://three", "stub://four",
                LQChannel1.ToString(), LQChannel2.ToString(), LQChannel3.ToString(), LQChannel4.ToString());
        }

        protected Type messageTypeFor(string name)
        {
            return messageTypes.First(x => x.Name == name);
        }
    }
}