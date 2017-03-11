using JasperBus.Tests.Compilation;
using Xunit;

namespace JasperBus.Tests.Bootstrapping
{
    /*
     * Test cases
     * DONE - 1. Find class suffixed as Handler
     * DONE - 2. Find class suffexed as Consumer
     * 3. Skip class marked as [NotHandler]
     * 4. Skip method marked as [NotHandler]
     * 5. Find static method
     * 6. Find instance method
     * 7. Prove that it is selecting on the message type
     * 8. Use a mix of parameters besides the message type
     * 9. Extra types suffixed by something else
     * 10. IncludeTypesImplementing<T>()
     * 11. IncludeMethods() -- is this really necessary?
     * 12. ExcludeTypes()
     * 13. IgnoreMethodsDeclaredBy<T>
     * 14. ExcludeNonConcreteTypes()
     * 15. Find for an interface that fits the naming
     */


    public class find_handlers_with_the_default_handler_discovery : IntegrationContext
    {
        public find_handlers_with_the_default_handler_discovery()
        {
            var registry = new JasperBusRegistry();
            registry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            with(registry);
        }

        [Fact]
        public void finds_classes_suffixed_as_Handler()
        {
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Handle(new MovieAdded()));
        }

        [Fact]
        public void finds_classes_suffixed_as_Consumer()
        {
            chainFor<Event1>().ShouldHaveHandler<EventConsumer>(x => x.Consume(new Event1()));
        }

    }

    public class MovieAdded{}
    public class MovieRemoved{}
    public class EpisodeAvailable{}
    public class NewShow{}

    public class NetflixHandler
    {
        public void Handle(MovieAdded added)
        {
            
        }


    }


    public class Event1 { }
    public class Event2 { }
    public class Event3 { }
    public class Event4 { }

    public class EventConsumer
    {
        public void Consume(Event1 @event)
        {
            
        }
    }
}