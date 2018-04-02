using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class find_handlers_with_the_default_handler_discovery : IntegrationContext
    {

        [Fact]
        public async Task can_find_appropriate_static_method()
        {
            await withAllDefaults();

            chainFor<MovieRemoved>().Handlers.Any().ShouldBeTrue();
        }

        [Fact]
        public async Task can_find_handlers_from_static_classes()
        {
            await withAllDefaults();
            chainFor<StaticClassMessage>().Handlers.Single().HandlerType
                .ShouldBe(typeof(StaticClassHandler));
        }

        [Fact]
        public async Task does_not_find_handlers_that_do_not_match_the_type_naming_convention()
        {
            await withAllDefaults();
            chainFor<MovieAdded>().ShouldNotHaveHandler<MovieWatcher>(x => x.Handle(null));
        }

        [Fact]
        public async Task finds_classes_suffixed_as_Consumer()
        {
            await withAllDefaults();
            chainFor<Event1>().ShouldHaveHandler<EventConsumer>(x => x.Consume(new Event1()));
        }

        [Fact]
        public async Task finds_classes_suffixed_as_Handler()
        {
            await withAllDefaults();
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Consume(new MovieAdded()));
        }

        [Fact]
        public async Task finds_interface_messages_too()
        {
            await withAllDefaults();
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Handles((IMovieEvent) null));
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Handles((MovieEvent) null));
            chainFor<MovieRemoved>().ShouldHaveHandler<NetflixHandler>(x => x.Handles((IMovieEvent) null));
            chainFor<MovieRemoved>().ShouldHaveHandler<NetflixHandler>(x => x.Handles((MovieEvent) null));
        }

        [Fact]
        public async Task ignore_class_marked_as_NotHandler()
        {
            await withAllDefaults();
            chainFor<MovieAdded>()
                .ShouldNotHaveHandler<BlockbusterHandler>(x => x.Handle(new MovieAdded()));
        }

        [Fact]
        public async Task ignore_method_marked_as_NotHandler()
        {
            await with(x => x.Handlers.DisableConventionalDiscovery().IncludeType<NetflixHandler>());
            //await withAllDefaults();
            chainFor<MovieAdded>()
                .ShouldNotHaveHandler<NetflixHandler>(x => x.Handles(new MovieAdded()));
        }

        [Fact]
        public async Task will_find_methods_with_parameters_other_than_the_message()
        {
            await withAllDefaults();
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Handle(null, null));
        }
    }

    public class customized_finding : IntegrationContext
    {
        [Fact]
        public async Task extra_suffix()
        {
            await with(x => x.Handlers.Discovery(d => d.IncludeClassesSuffixedWith("Watcher")));

            chainFor<MovieAdded>().ShouldHaveHandler<MovieWatcher>(x => x.Handle(null));
        }

        [Fact]
        public async Task handler_types_from_a_marker_interface()
        {
            await with(x => x.Handlers.Discovery(d => d.IncludeTypesImplementing<IMovieThing>()));

            chainFor<MovieAdded>().ShouldHaveHandler<EpisodeWatcher>(x => x.Handle(new MovieAdded()));
        }
    }

    public interface IMovieSink
    {
        void Listen(MovieAdded added);
    }

    public interface IMovieThing
    {
    }

    public class EpisodeWatcher : IMovieThing
    {
        public void Handle(MovieAdded added)
        {
        }
    }

    public abstract class MovieEvent : IMovieEvent
    {
    }

    public class MovieAdded : MovieEvent
    {
    }

    public class MovieRemoved : MovieEvent
    {
    }

    public class EpisodeAvailable
    {
    }

    public class NewShow
    {
    }

    public interface IMovieEvent
    {
    }

    public class MovieWatcher
    {
        public void Handle(MovieAdded added)
        {
        }
    }

    public class StaticClassMessage
    {
    }

    public static class StaticClassHandler
    {
        public static void Handle(StaticClassMessage message)
        {
        }
    }

    // SAMPLE: JasperIgnoreAttribute
    public class NetflixHandler : IMovieSink
    {
        public void Listen(MovieAdded added)
        {
        }

        public void Handles(IMovieEvent @event)
        {
        }

        public void Handles(MovieEvent @event)
        {
        }

        public void Consume(MovieAdded added)
        {
        }

        // Only this method will be ignored as
        // a handler method
        [JasperIgnore]
        public void Handles(MovieAdded added)
        {
        }

        public void Handle(MovieAdded message, IMessageContext context)
        {
        }

        public static Task Handle(MovieRemoved removed)
        {
            return Task.CompletedTask;
        }
    }

    // All methods on this class will be ignored
    // as handler methods even though the class
    // name matches the discovery naming conventions
    [JasperIgnore]
    public class BlockbusterHandler
    {
        public void Handle(MovieAdded added)
        {
        }
    }
    // ENDSAMPLE


    public class Event1
    {
    }

    public class Event2
    {
    }

    public class Event3
    {
    }

    public class Event4
    {
    }

    public class EventConsumer
    {
        public void Consume(Event1 @event)
        {
        }
    }
}
