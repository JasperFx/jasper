using System;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Conneg;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Samples
{

    namespace FirstTry
    {
        // SAMPLE: PersonBorn1
        public class PersonBorn
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

            // This is obviously a contrived example
            // so just let this go for now;)
            public int Day { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
        }
        // ENDSAMPLE


        public class message_alias
        {
            // SAMPLE: ootb-message-alias
            [Fact]
            public void message_alias_is_fullname_by_default()
            {
                new Envelope(new PersonBorn())
                    .MessageType.ShouldBe(typeof(PersonBorn).FullName);
            }
            // ENDSAMPLE
        }
    }

    namespace SecondTry
    {
        // SAMPLE: override-message-alias
        [MessageAlias("person-born")]
        public class PersonBorn
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Day { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
        }
        // ENDSAMPLE

        public class message_alias
        {
            // SAMPLE: explicit-message-alias
            [Fact]
            public void message_alias_is_fullname_by_default()
            {
                new Envelope(new PersonBorn())
                    .MessageType.ShouldBe("person-born");
            }
            // ENDSAMPLE
        }
    }

    namespace ThirdTry
    {
        // SAMPLE: PersonBorn-V2
        [MessageAlias("person-born"), Version("V2")]
        public class PersonBornV2
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime Birthday { get; set; }
        }
        // ENDSAMPLE

        // SAMPLE: IForwardsTo<PersonBornV2>
        [Version("V1")]
        public class PersonBorn : IForwardsTo<PersonBornV2>
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Day { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }

            public PersonBornV2 Transform()
            {
                return new PersonBornV2
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Birthday = new DateTime(Year, Month, Day)
                };
            }
        }
        // ENDSAMPLE

        // SAMPLE: PersonCreatedHandler
        public class PersonCreatedHandler
        {
            public static void Handle(PersonBorn person)
            {
                // do something w/ the message
            }

            public static void Handle(PersonBornV2 person)
            {
                // do something w/ the message
            }
        }
        // ENDSAMPLE
    }

    // SAMPLE: NoUnVersionedMessages
    public class NoUnVersionedMessages : JasperRegistry
    {
        public NoUnVersionedMessages()
        {
            Advanced.AllowNonVersionedSerialization = false;
        }
    }
    // ENDSAMPLE

    // SAMPLE: RegisteringCustomReadersAndWriters
    public class RegisteringCustomReadersAndWriters : JasperRegistry
    {
        public RegisteringCustomReadersAndWriters()
        {
            Services.AddService<IMessageSerializer, MyCustomWriter>();
            Services.AddService<IMessageDeserializer, MyCustomReader>();
        }
    }
    // ENDSAMPLE

    public class MyCustomWriter : IMessageSerializer
    {
        public Type DotNetType { get; }
        public string ContentType { get; }
        public byte[] Write(object model)
        {
            throw new NotImplementedException();
        }

        public Task WriteToStream(object model, HttpResponse response)
        {
            throw new NotImplementedException();
        }
    }


    public class MyCustomReader : IMessageDeserializer
    {
        public string MessageType { get; }
        public Type DotNetType { get; }
        public string ContentType { get; }

        public object ReadFromData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            throw new NotImplementedException();
        }
    }

    // SAMPLE: CustomizingJsonSerialization
    public class CustomizingJsonSerialization : JasperRegistry
    {
        public CustomizingJsonSerialization()
        {
            Settings.Alter<BusSettings>(_ =>
            {
                // Edit the JsonSerializerSettings used by the messaging serialization
                _.JsonSerialization.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            });
        }
    }
    // ENDSAMPLE

}
