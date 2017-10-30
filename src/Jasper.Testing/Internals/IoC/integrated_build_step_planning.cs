using System;
using Jasper.Bus.Model;
using Jasper.Testing.Bus;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Http;
using Jasper.Testing.Internals.TargetTypes;
using Microsoft.Extensions.DependencyInjection;
using Red;
using Xunit;
using IWidget = Jasper.Testing.Bus.Compilation.IWidget;

namespace Jasper.Testing.Internals.IoC
{
    public class integrated_build_step_planning : IDisposable
    {
        private JasperRuntime _runtime;
        private readonly JasperRegistry theRegistry = new JasperRegistry();



        public void Dispose()
        {
            _runtime?.Dispose();
        }

        private string codeFor<T>()
        {
            if (_runtime == null)
            {
                theRegistry.Http.Actions.DisableConventionalDiscovery();
                theRegistry.Handlers.DisableConventionalDiscovery();
                _runtime = JasperRuntime.For(theRegistry);
            }

            return _runtime.Get<HandlerGraph>().ChainFor<T>().SourceCode;
        }

        [Fact]
        public void try_single_handler_no_args()
        {
            theRegistry.Handlers.IncludeType<NoArgMethod>();

            var code = codeFor<Message1>();

            code.ShouldContain("var noArgMethod = new Jasper.Testing.Internals.IoC.NoArgMethod();");
        }

        [Fact]
        public void try_single_handler_one_singleton_arg()
        {
            theRegistry.Handlers.IncludeType<SingletonArgMethod>();

            theRegistry.Services.AddSingleton(new MessageTracker());

            var code = codeFor<Message1>();

            code.ShouldContain("public Red_Message1(MessageTracker messageTracker)");
            code.ShouldContain("var singletonArgMethod = new Jasper.Testing.Internals.IoC.SingletonArgMethod(_messageTracker);");
        }

        [Fact]
        public void try_single_handler_two_singleton_arg()
        {
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddSingleton(new MessageTracker());
            theRegistry.Services.AddSingleton<IWidget, Widget>();


            var code = codeFor<Message1>();

            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.Internals.IoC.MultipleArgMethod(_messageTracker, _widget);");
        }

        [Fact]
        public void try_single_handler_two_container_scoped_args()
        {
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddScoped<MessageTracker>();
            theRegistry.Services.AddScoped<IWidget, Widget>();


            var code = codeFor<Message1>();

            code.ShouldContain("var widget = new Jasper.Testing.Bus.Compilation.Widget();");
            code.ShouldContain("var messageTracker = new Jasper.Testing.Bus.MessageTracker();");
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.Internals.IoC.MultipleArgMethod(messageTracker, widget);");
        }

        [Fact]
        public void try_single_handler_two_container_scoped_args_one_disposable()
        {
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddScoped<MessageTracker>();
            theRegistry.Services.AddScoped<IWidget, DisposedWidget>();


            var code = codeFor<Message1>();

            code.ShouldContain("using (var widget = new Jasper.Testing.Internals.IoC.DisposedWidget())");
            code.ShouldContain("var messageTracker = new Jasper.Testing.Bus.MessageTracker();");
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.Internals.IoC.MultipleArgMethod(messageTracker, widget);");
        }

        [Fact]
        public void try_single_handler_two_container_scoped_args_both_disposable()
        {
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddScoped<MessageTracker, DisposedMessageTracker>();
            theRegistry.Services.AddScoped<IWidget, DisposedWidget>();


            var code = codeFor<Message1>();

            code.ShouldContain("using (var messageTracker = new Jasper.Testing.Internals.IoC.DisposedMessageTracker())");
            code.ShouldContain("using (var widget = new Jasper.Testing.Internals.IoC.DisposedWidget())");
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.Internals.IoC.MultipleArgMethod(messageTracker, widget);");
        }

        [Fact]
        public void try_single_handler_one_transient_arg()
        {
            theRegistry.Handlers.IncludeType<SingletonArgMethod>();

            theRegistry.Services.AddTransient<MessageTracker>();

            var code = codeFor<Message1>();

            code.ShouldContain("var messageTracker = new Jasper.Testing.Bus.MessageTracker();");
            code.ShouldContain("var singletonArgMethod = new Jasper.Testing.Internals.IoC.SingletonArgMethod(messageTracker);");
        }

        [Fact]
        public void try_single_handler_one_transient_arg_that_is_disposable()
        {
            theRegistry.Handlers.IncludeType<SingletonArgMethod>();

            theRegistry.Services.AddTransient<MessageTracker, DisposedMessageTracker>();

            var code = codeFor<Message1>();

            code.ShouldContain("using (var messageTracker = new Jasper.Testing.Internals.IoC.DisposedMessageTracker())");
            code.ShouldContain("var singletonArgMethod = new Jasper.Testing.Internals.IoC.SingletonArgMethod(messageTracker);");
        }

        [Fact]
        public void multiple_actions_using_the_same_transient()
        {
            theRegistry.Handlers.IncludeType<WidgetUser>();
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddTransient<IWidget, Widget>();
            theRegistry.Services.AddSingleton(new MessageTracker());

            var code = codeFor<Message1>();

            code.ShouldContain("var widget1 = new Jasper.Testing.Bus.Compilation.Widget();");
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.Internals.IoC.MultipleArgMethod(_messageTracker, widget1);");
            code.ShouldContain("var widget2 = new Jasper.Testing.Bus.Compilation.Widget();");
            code.ShouldContain("var widgetUser = new Jasper.Testing.Internals.IoC.WidgetUser(widget2);");
        }

        [Fact]
        public void multiple_actions_using_the_same_scoped()
        {
            theRegistry.Handlers.IncludeType<WidgetUser>();
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddScoped<IWidget, Widget>();
            theRegistry.Services.AddSingleton(new MessageTracker());

            var code = codeFor<Message1>();

            code.ShouldContain("var widget = new Jasper.Testing.Bus.Compilation.Widget();");
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.Internals.IoC.MultipleArgMethod(_messageTracker, widget);");
            code.ShouldContain("var widgetUser = new Jasper.Testing.Internals.IoC.WidgetUser(widget);");
        }

        [Fact]
        public void multiple_actions_using_the_same_disposable_transient()
        {
            theRegistry.Handlers.IncludeType<WidgetUser>();
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddTransient<IWidget, DisposedWidget>();
            theRegistry.Services.AddSingleton(new MessageTracker());

            var code = codeFor<Message1>();


            code.ShouldContain("using (var widget1 = new Jasper.Testing.Internals.IoC.DisposedWidget())");
            code.ShouldContain("using (var widget2 = new Jasper.Testing.Internals.IoC.DisposedWidget())");
        }

        [Fact]
        public void multiple_actions_one_cannot_be_reduced()
        {
            // This is enough to make it not be reduceable
            theRegistry.Services.AddTransient<IWidget>(s => null);
            theRegistry.Services.AddSingleton(new MessageTracker());

            theRegistry.Handlers.IncludeType<SingletonArgMethod>();
            theRegistry.Handlers.IncludeType<WidgetUser>();

            var code = codeFor<Message1>();

            code.ShouldContain("using (var serviceScope = _serviceScopeFactory.CreateScope())");
            code.ShouldContain("var widgetUser = (Jasper.Testing.Internals.IoC.WidgetUser)serviceProvider.GetService(typeof(Jasper.Testing.Internals.IoC.WidgetUser));");
            code.ShouldContain("var singletonArgMethod = (Jasper.Testing.Internals.IoC.SingletonArgMethod)serviceProvider.GetService(typeof(Jasper.Testing.Internals.IoC.SingletonArgMethod));");
        }

        // TODO -- Eliminate this functional gap
        [Fact]
        public void cannot_reduce_an_enumeration_YET()
        {
            theRegistry.Handlers.IncludeType<WidgetArrayUser>();

            var code = codeFor<Message1>();

            code.ShouldContain("var widgetArrayUser = (Jasper.Testing.Internals.IoC.WidgetArrayUser)serviceProvider.GetService(typeof(Jasper.Testing.Internals.IoC.WidgetArrayUser));");
        }

        [Fact]
        public void use_a_known_variable_in_the_mix()
        {
            theRegistry.Handlers.IncludeType<UsesKnownServiceThing>();
            theRegistry.Services.AddSingleton<IFakeStore, FakeStore>();

            var code = codeFor<Message1>();

            code.ShouldContain("using (var session = _fakeStore.OpenSession())");
            code.ShouldContain("var usesKnownServiceThing = new Jasper.Testing.Internals.IoC.UsesKnownServiceThing(session);");
        }

        [Fact]
        public void cannot_reduce_with_middleware_that_cannot_be_reduced()
        {
            theRegistry.Handlers.IncludeType<UsesKnownServiceThing>();
            theRegistry.Services.AddScoped<IFakeStore>(s => null);

            var code = codeFor<Message1>();

            code.ShouldContain("var fakeStore = (Jasper.Testing.FakeStoreTypes.IFakeStore)serviceProvider.GetService(typeof(Jasper.Testing.FakeStoreTypes.IFakeStore));");
        }
    }

    [FakeTransaction]
    public class UsesKnownServiceThing
    {
        public UsesKnownServiceThing(IFakeSession session)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class WidgetArrayUser
    {
        public WidgetArrayUser(IWidget[] widgets)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class DisposedMessageTracker : MessageTracker, IDisposable
    {
        public void Dispose()
        {

        }
    }

    public class NoArgMethod
    {
        public void Handle(Message1 message)
        {

        }
    }

    public class SingletonArgMethod
    {
        public SingletonArgMethod(MessageTracker tracker)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }



    public class MultipleArgMethod
    {
        public MultipleArgMethod(MessageTracker tracker, IWidget widget)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class WidgetUser
    {
        public WidgetUser(IWidget widget)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class DisposedWidget : IWidget, IDisposable
    {
        public void Dispose()
        {

        }
    }
}
