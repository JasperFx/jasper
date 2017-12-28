using System;
using System.Collections.Generic;
using Jasper.Bus.Model;
using Jasper.Testing.Bus;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Microsoft.Extensions.DependencyInjection;
using Red;
using Xunit;
using IWidget = Jasper.Testing.Bus.Compilation.IWidget;

namespace Jasper.Testing
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

            code.ShouldContain("var noArgMethod = new Jasper.Testing.NoArgMethod();");
        }

        [Fact]
        public void try_single_handler_one_singleton_arg()
        {
            theRegistry.Handlers.IncludeType<SingletonArgMethod>();

            theRegistry.Services.AddSingleton(new MessageTracker());

            var code = codeFor<Message1>();

            code.ShouldContain("public Red_Message1(MessageTracker messageTracker)");
            code.ShouldContain("var singletonArgMethod = new Jasper.Testing.SingletonArgMethod(_messageTracker);");
        }

        [Fact]
        public void try_single_handler_two_singleton_arg()
        {
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddSingleton(new MessageTracker());
            theRegistry.Services.AddSingleton<IWidget, Widget>();


            var code = codeFor<Message1>();

            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.MultipleArgMethod(_messageTracker, _widget);");
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
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.MultipleArgMethod(messageTracker, widget);");
        }

        [Fact]
        public void try_single_handler_two_container_scoped_args_one_disposable()
        {
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddScoped<MessageTracker>();
            theRegistry.Services.AddScoped<IWidget, DisposedWidget>();


            var code = codeFor<Message1>();

            code.ShouldContain("using (var widget = new Jasper.Testing.DisposedWidget())");
            code.ShouldContain("var messageTracker = new Jasper.Testing.Bus.MessageTracker();");
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.MultipleArgMethod(messageTracker, widget);");
        }

        [Fact]
        public void try_single_handler_two_container_scoped_args_both_disposable()
        {
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddScoped<MessageTracker, DisposedMessageTracker>();
            theRegistry.Services.AddScoped<IWidget, DisposedWidget>();


            var code = codeFor<Message1>();

            code.ShouldContain("using (var messageTracker = new Jasper.Testing.DisposedMessageTracker())");
            code.ShouldContain("using (var widget = new Jasper.Testing.DisposedWidget())");
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.MultipleArgMethod(messageTracker, widget);");
        }

        [Fact]
        public void try_single_handler_one_transient_arg()
        {
            theRegistry.Handlers.IncludeType<SingletonArgMethod>();

            theRegistry.Services.AddTransient<MessageTracker>();

            var code = codeFor<Message1>();

            code.ShouldContain("var messageTracker = new Jasper.Testing.Bus.MessageTracker();");
            code.ShouldContain("var singletonArgMethod = new Jasper.Testing.SingletonArgMethod(messageTracker);");
        }

        [Fact]
        public void try_single_handler_one_transient_arg_that_is_disposable()
        {
            theRegistry.Handlers.IncludeType<SingletonArgMethod>();

            theRegistry.Services.AddTransient<MessageTracker, DisposedMessageTracker>();

            var code = codeFor<Message1>();

            code.ShouldContain("using (var messageTracker = new Jasper.Testing.DisposedMessageTracker())");
            code.ShouldContain("var singletonArgMethod = new Jasper.Testing.SingletonArgMethod(messageTracker);");
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
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.MultipleArgMethod(_messageTracker, widget1);");
            code.ShouldContain("var widget2 = new Jasper.Testing.Bus.Compilation.Widget();");
            code.ShouldContain("var widgetUser = new Jasper.Testing.WidgetUser(widget2);");
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
            code.ShouldContain("var multipleArgMethod = new Jasper.Testing.MultipleArgMethod(_messageTracker, widget);");
            code.ShouldContain("var widgetUser = new Jasper.Testing.WidgetUser(widget);");
        }

        [Fact]
        public void multiple_actions_using_the_same_disposable_transient()
        {
            theRegistry.Handlers.IncludeType<WidgetUser>();
            theRegistry.Handlers.IncludeType<MultipleArgMethod>();

            theRegistry.Services.AddTransient<IWidget, DisposedWidget>();
            theRegistry.Services.AddSingleton(new MessageTracker());

            var code = codeFor<Message1>();


            code.ShouldContain("using (var widget1 = new Jasper.Testing.DisposedWidget())");
            code.ShouldContain("using (var widget2 = new Jasper.Testing.DisposedWidget())");
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
            code.ShouldContain("var widgetUser = (Jasper.Testing.WidgetUser)serviceProvider.GetService(typeof(Jasper.Testing.WidgetUser));");
            code.ShouldContain("var singletonArgMethod = (Jasper.Testing.SingletonArgMethod)serviceProvider.GetService(typeof(Jasper.Testing.SingletonArgMethod));");
        }



        [Fact]
        public void use_a_known_variable_in_the_mix()
        {
            theRegistry.Handlers.IncludeType<UsesKnownServiceThing>();
            theRegistry.Services.AddSingleton<IFakeStore, FakeStore>();

            var code = codeFor<Message1>();

            code.ShouldContain("using (var session = _fakeStore.OpenSession())");
            code.ShouldContain("var usesKnownServiceThing = new Jasper.Testing.UsesKnownServiceThing(session);");
        }

        [Fact]
        public void cannot_reduce_with_middleware_that_cannot_be_reduced()
        {
            theRegistry.Handlers.IncludeType<UsesKnownServiceThing>();
            theRegistry.Services.AddScoped<IFakeStore>(s => null);

            var code = codeFor<Message1>();

            code.ShouldContain("var fakeStore = (Jasper.Testing.FakeStoreTypes.IFakeStore)serviceProvider.GetService(typeof(Jasper.Testing.FakeStoreTypes.IFakeStore));");
        }

        [Fact]
        public void can_reduce_with_closed_generic_service_dependency()
        {
            theRegistry.Handlers.IncludeType<GenericServiceUsingMethod<string>>();
            theRegistry.Services.AddSingleton(new MessageTracker());
            theRegistry.Services.AddTransient(typeof(IService<>), typeof(Service<>));

            var code = codeFor<Message1>();

            code.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            code.ShouldContain("var genericServiceUsingMethod = new Jasper.Testing.GenericServiceUsingMethod<System.String>();");

            code.ShouldContain("var service = new Jasper.Testing.Service<System.String>(_messageTracker);");

        }

        [Fact]
        public void can_reduce_with_array_dependency()
        {
            theRegistry.Handlers.IncludeType<HandlerWithArray>();

            theRegistry.Services.AddTransient<IWidget, RedWidget>();
            theRegistry.Services.AddScoped<IWidget, GreenWidget>();
            theRegistry.Services.AddScoped<IWidget, BlueWidget>();

            var code = codeFor<Message1>();

            code.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            code.ShouldContain("var widgetArray = new Jasper.Testing.Bus.Compilation.IWidget[]{widget3, widget2, widget1};");
            code.ShouldContain("var handlerWithArray = new Jasper.Testing.HandlerWithArray(widgetArray);");
        }

        [Fact]
        public void use_registered_array_if_one_is_known()
        {
            theRegistry.Handlers.IncludeType<HandlerWithArray>();

            theRegistry.Handlers.IncludeType<HandlerWithArray>();
            theRegistry.Services.AddSingleton<IWidget[]>(new IWidget[] {new BlueWidget(), new GreenWidget(),});

            var code = codeFor<Message1>();

            code.ShouldContain("public Red_Message1(IWidget[] widgetArray)");
            code.ShouldContain("var handlerWithArray = new Jasper.Testing.HandlerWithArray(_widgetArray);");
        }

        [Fact]
        public void can_reduce_with_enumerable_dependency()
        {
            theRegistry.Handlers.IncludeType<WidgetEnumerableUser>();

            theRegistry.Services.AddTransient<IWidget, RedWidget>();
            theRegistry.Services.AddScoped<IWidget, GreenWidget>();
            theRegistry.Services.AddScoped<IWidget, BlueWidget>();

            var code = codeFor<Message1>();

            code.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            code.ShouldContain("var widgetList = new System.Collections.Generic.List<Jasper.Testing.Bus.Compilation.IWidget>{widget3, widget2, widget1};");
        }

        [Fact]
        public void use_registered_enumerable_if_one_is_known()
        {
            theRegistry.Handlers.IncludeType<WidgetEnumerableUser>();

            theRegistry.Services.AddSingleton<IEnumerable<IWidget>>(new IWidget[] {new BlueWidget(), new GreenWidget(),});

            var code = codeFor<Message1>();

            code.ShouldContain("public Red_Message1(IEnumerable<Jasper.Testing.Bus.Compilation.IWidget> widgetEnumerable)");
            code.ShouldContain("var widgetEnumerableUser = new Jasper.Testing.WidgetEnumerableUser(_widgetEnumerable);");
        }




        [Fact]
        public void can_reduce_with_list_dependency()
        {
            theRegistry.Handlers.IncludeType<WidgetListUser>();

            theRegistry.Services.AddTransient<IWidget, RedWidget>();
            theRegistry.Services.AddScoped<IWidget, GreenWidget>();
            theRegistry.Services.AddScoped<IWidget, BlueWidget>();

            var code = codeFor<Message1>();

            code.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            code.ShouldContain("var widgetList = new System.Collections.Generic.List<Jasper.Testing.Bus.Compilation.IWidget>{widget3, widget2, widget1};");
        }

        [Fact]
        public void use_registered_list_if_one_is_known()
        {
            theRegistry.Handlers.IncludeType<WidgetListUser>();

            theRegistry.Services.AddSingleton<List<IWidget>>(new List<IWidget> {new BlueWidget(), new GreenWidget(),});

            var code = codeFor<Message1>();

            code.ShouldContain("public Red_Message1(List<Jasper.Testing.Bus.Compilation.IWidget> widgetList)");
            code.ShouldContain("var widgetListUser = new Jasper.Testing.WidgetListUser(_widgetList);");
        }

        [Fact]
        public void can_reduce_with_IList_dependency()
        {
            theRegistry.Handlers.IncludeType<WidgetIListUser>();

            theRegistry.Services.AddTransient<IWidget, RedWidget>();
            theRegistry.Services.AddScoped<IWidget, GreenWidget>();
            theRegistry.Services.AddScoped<IWidget, BlueWidget>();

            var code = codeFor<Message1>();

            code.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            code.ShouldContain("var widgetList = new System.Collections.Generic.List<Jasper.Testing.Bus.Compilation.IWidget>{widget3, widget2, widget1};");
        }

        [Fact]
        public void use_registered_Ilist_if_one_is_known()
        {
            theRegistry.Handlers.IncludeType<WidgetIListUser>();

            theRegistry.Services.AddSingleton<IList<IWidget>>(new List<IWidget> {new BlueWidget(), new GreenWidget(),});

            var code = codeFor<Message1>();

            code.ShouldContain("public Red_Message1(IList<Jasper.Testing.Bus.Compilation.IWidget> widgetList)");
            code.ShouldContain("var widgetIListUser = new Jasper.Testing.WidgetIListUser(_widgetList);");
        }

        [Fact]
        public void can_reduce_with_IReadOnlyList_dependency()
        {
            theRegistry.Handlers.IncludeType<WidgetIReadOnlyListUser>();

            theRegistry.Services.AddTransient<IWidget, RedWidget>();
            theRegistry.Services.AddScoped<IWidget, GreenWidget>();
            theRegistry.Services.AddScoped<IWidget, BlueWidget>();

            var code = codeFor<Message1>();

            code.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            code.ShouldContain("var widgetList = new System.Collections.Generic.List<Jasper.Testing.Bus.Compilation.IWidget>{widget3, widget2, widget1};");
        }

        [Fact]
        public void use_registered_IReadOnlyList_if_one_is_known()
        {
            theRegistry.Handlers.IncludeType<WidgetIReadOnlyListUser>();

            theRegistry.Services.AddSingleton<IReadOnlyList<IWidget>>(new List<IWidget> {new BlueWidget(), new GreenWidget(),});

            var code = codeFor<Message1>();

            code.ShouldContain("public Red_Message1(IReadOnlyList<Jasper.Testing.Bus.Compilation.IWidget> widgetList)");
            code.ShouldContain("var widgetIReadOnlyListUser = new Jasper.Testing.WidgetIReadOnlyListUser(_widgetList);");
        }
    }


    public class RedWidget : IWidget{}
    public class GreenWidget : IWidget{}
    public class BlueWidget : IWidget{}

    public class HandlerWithArray
    {
        public HandlerWithArray(IWidget[] widgets)
        {
        }

        public void Handle(Message1 message)
        {

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

    public class WidgetEnumerableUser
    {
        public WidgetEnumerableUser(IEnumerable<IWidget> widgets)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class WidgetListUser
    {
        public WidgetListUser(List<IWidget> widgets)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class WidgetIListUser
    {
        public WidgetIListUser(IList<IWidget> widgets)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class WidgetIReadOnlyListUser
    {
        public WidgetIReadOnlyListUser(IReadOnlyList<IWidget> widgets)
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

    public class GenericServiceUsingMethod<T>
    {
        public void Handle(Message1 message, IService<T> service)
        {

        }
    }

    public interface IService<T>{}

    public class Service<T> : IService<T>
    {
        public Service(MessageTracker tracker)
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
