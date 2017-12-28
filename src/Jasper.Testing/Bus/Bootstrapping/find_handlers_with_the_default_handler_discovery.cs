using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jasper.Bus.Runtime.Invocation;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Shouldly;
using StructureMap.TypeRules;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class find_handlers_with_the_default_handler_discovery : IntegrationContext
    {
        public find_handlers_with_the_default_handler_discovery()
        {
            withAllDefaults();
        }


        [Fact]
        public void can_find_appropriate_static_method()
        {
            chainFor<MovieRemoved>().ShouldHaveHandler<NetflixHandler>(nameof(NetflixHandler.HandleAsync));
        }

        [Fact]
        public void finds_classes_suffixed_as_Consumer()
        {
            chainFor<Event1>().ShouldHaveHandler<EventConsumer>(x => x.Consume(new Event1()));
        }

        [Fact]
        public void finds_classes_suffixed_as_Handler()
        {
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Handle(new MovieAdded()));
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
        public void will_find_methods_with_parameters_other_than_the_message()
        {
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Handle3(null, null));
        }

        [Fact]
        public void finds_interface_messages_too()
        {
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Record(null));
            chainFor<MovieAdded>().ShouldHaveHandler<NetflixHandler>(x => x.Record2(null));
            chainFor<MovieRemoved>().ShouldHaveHandler<NetflixHandler>(x => x.Record(null));
            chainFor<MovieRemoved>().ShouldHaveHandler<NetflixHandler>(x => x.Record2(null));
        }

        [Fact]
        public void does_not_find_handlers_that_do_not_match_the_type_naming_convention()
        {
            chainFor<MovieAdded>().ShouldNotHaveHandler<MovieWatcher>(x => x.Watch(null));
        }

        [Fact]
        public void can_find_handlers_from_static_classes()
        {
            chainFor<StaticClassMessage>().Handlers.Single().HandlerType
                .ShouldBe(typeof(StaticClassHandler));
        }

    }

    public class customized_finding : IntegrationContext
    {
        [Fact]
        public void extra_suffix()
        {
            with(x => x.Handlers.IncludeClassesSuffixedWith("Watcher"));

            chainFor<MovieAdded>().ShouldHaveHandler<MovieWatcher>(x => x.Watch(null));
        }

        [Fact]
        public void handler_types_from_a_marker_interface()
        {
            with(x => x.Handlers.IncludeTypesImplementing<IMovieThing>());

            chainFor<MovieAdded>().ShouldHaveHandler<EpisodeWatcher>(x => x.Handle(new MovieAdded()));
        }

    }

    public interface IMovieSink
    {
        void Listen(MovieAdded added);
    }

    public interface IMovieThing { }

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
        public void Watch(MovieAdded added)
        {

        }
    }

    public class StaticClassMessage{}

    public static class StaticClassHandler
    {
        public static void Handle(StaticClassMessage message)
        {

        }
    }

    // SAMPLE: JasperIgnoreAttribute
    public class NetflixHandler : IMovieSink
    {
        public void Record(IMovieEvent @event)
        {

        }

        public void Record2(MovieEvent @event)
        {

        }

        public void Handle(MovieAdded added)
        {
        }

        // Only this method will be ignored as
        // a handler method
        [JasperIgnore]
        public void Handle2(MovieAdded added)
        {
        }

        public void Handle3(MovieAdded message, IInvocationContext context)
        {
        }

        public static Task HandleAsync(MovieRemoved removed)
        {
            return Task.CompletedTask;
        }

        public void Listen(MovieAdded added)
        {
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
