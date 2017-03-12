using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using JasperBus.Model;
using JasperBus.Runtime.Invocation;
using JasperBus.Tests.Compilation;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Bootstrapping
{



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

        [Fact]
        public void ignore_class_marked_as_NotHandler()
        {
            chainFor<MovieAdded>()
                .ShouldNotHaveHandler<BlockbusterHandler>(x => x.Handle(new MovieAdded()));
        }

        [Fact]
        public void ignore_method_marked_as_NotHandler()
        {
            chainFor<MovieAdded>()
                .ShouldNotHaveHandler<NetflixHandler>(x => x.Handle2(new MovieAdded()));
        }


        [Fact]
        public void can_find_appropriate_static_method()
        {
            chainFor<MovieRemoved>().ShouldHaveHandler<NetflixHandler>(nameof(NetflixHandler.HandleAsync));
        }

        [Fact]
        public void will_find_methods_with_parameters_other_than_the_message()
        {
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Handle3(null, null));
        }



        /*
         * Test cases
         * DONE - 1. Find class suffixed as Handler
         * DONE - 2. Find class suffexed as Consumer
         * DONE - 3. Skip class marked as [NotHandler]
         * DONE - 4. Skip method marked as [NotHandler]
         * DONE - 5. Find static method
         * COVERED - 6. Find instance method
         * COVERED - 7. Prove that it is selecting on the message type
         * DONE - 8. Use a mix of parameters besides the message type
         * 9. Extra types suffixed by something else
         * 10. IncludeTypesImplementing<T>()
         * 11. IncludeMethods() -- is this really necessary?
         * 12. ExcludeTypes()
         * 13. IgnoreMethodsDeclaredBy<T>
         * 14. ExcludeNonConcreteTypes()
         * 15. Find for an interface that fits the naming
         * 16. Multiple handler calls for the same message type
         * 17. Adds the abstract and interface message handlers at the end
         */
    }

    public class MovieAdded : IMovieEvent{}
    public class MovieRemoved : IMovieEvent{}
    public class EpisodeAvailable{}
    public class NewShow{}
    public interface IMovieEvent{}


    public class NetflixHandler
    {
        public void Handle(MovieAdded added)
        {
            
        }

        [NotHandler]
        public void Handle2(MovieAdded added)
        {

        }

        public void Handle3(MovieAdded message, IEnvelopeContext context)
        {

        }

        public static Task HandleAsync(MovieRemoved removed)
        {
            return Task.CompletedTask;
        }


    }

    [NotHandler]
    public class BlockbusterHandler
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