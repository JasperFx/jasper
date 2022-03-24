<!--title:Jasper Middleware and Policies-->

::: tip warning
Jasper is the successor to an earlier project called [FubuMVC](https://fubumvc.github.io) that focused very hard on
user-defined conventions and a modular runtime pipeline. Great, but some users overused those features resulting in systems that were unmaintainable. While Jasper
definitely still supports user defined conventions and a flexible runtime pipeline, we urge some caution about overusing these
features when just some explicit code will do the job.
:::

## Jasper's Architectural Model

Internally, the key components of Jasper's architecture relevant to this section are shown below:

<[img:content/jaspermodel.png;Jasper's Configuration Model]>


Likewise, for the message handling:

* `HandlerChain` is the configuration-time model for each message handled by a Jasper application, including all the action methods and middleware
* `MessageHandler` is the common base class for the runtime-generated classes that handle messages at runtime. These classes are generated based
   on the configuration of its matching `HandlerChain`
* `HandlerGraph` is a collection of all the `HandlerChain` objects for the system
* `IHandlerPolicy` provides a facility to allow for user-defined conventions or policies by directly modifying the `HandlerGraph` model before the 
  `MessageHandler` classes are generated

Jasper's internal middleware is the [Frame model](https://jasperfx.github.io/lamar/documentation/compilation/frames/) from the closely related [Lamar](https://jasperfx.github.io/lamar) project. During application bootstrapping, Jasper builds up both the `RouteGraph` and `HandlerGraph` models that essentially model a collection of Lamar `Frame` objects that get compiled into the `RouteHandler` and `MessageHandler` classes at runtime.

To apply middleware to either HTTP routes or message handling, you'll be working with the common `IChain` interface shown below:

snippet: sample_IChain


For the HTTP routes in `Jasper.Http`:

* `RouteChain` is the configuration-time model for the Url pattern, the endpoint action method that executes the route, and any configured middleware 
   or post processing operations
* `RouteHandler` is the common base class for handling the HTTP routes. These classes are generated based on the configuration of its matching `RouteChain`
* `RouteGraph` is a collection of all the `RouteChain` objects for the system
* `IRoutePolicy` provides a facility to allow for user-defined conventions or policies by directly modifying the `RouteGraph` model before the 
  `RouteHandler` classes are generated



## Authoring Middleware

::: tip warning
This whole code compilation model is pretty new and there aren't enough examples yet. Feel very free to ask questions in the Gitter room linked in the top bar of this page.
:::

Jasper supports the "Russian Doll" model of middleware, similar in concept to ASP.Net Core but very different in implementation. Jasper's middleware uses runtime code generation and compilation with [LamarCompiler](https://jasperfx.github.io/lamar/documentation/compilation/). What this means is that "middleware" in Jasper is code that is woven right into the message and route handlers.

As an example, let's say you want to build some custom middleware that is a simple performance timing of either HTTP route execution or message execution. In essence, you want to inject code like this:

snippet: sample_stopwatch_concept

Alright, the first step is to create a LamarCompiler `Frame` class that generates that code around the inner message or HTTP handler:

snippet: sample_StopwatchFrame



## Applying Middleware

Okay, great, but the next question is "how do I stick this middleware on routes or message handlers?". You've got three options:

1. Use custom attributes 
1. Use a custom `IRoutePolicy` or `IHandlerPolicy` class
1. Expose a static `Configure(chain)` method on handler classes 

Even though one of the original design goals of FubuMVC and now Jasper was to eliminate or at least reduce the number of attributes users had to spew out into their application code, let's start with using an attribute.

## Custom Attributes

To attach our `StopwatchFrame` as middleware to any route or message handler, we can write a custom attribute based on Jasper's 
`ModifyChainAttribute` class as shown below:

snippet: sample_StopwatchAttribute

This attribute can now be placed either on a specific HTTP route endpoint method or message handler method to **only** apply to
that specific action, or it can be placed on a `Handler` or `Endpoint` class to apply to all methods exported by that type. 

Here's an example:

snippet: sample_ClockedEndpoint

Now, when the application is bootstrapped, this is the code that would be generated to handle the "GET /clocked" route:

```
    public class Jasper_Testing_Samples_ClockedEndpoint_get_clocked : Jasper.Http.Model.RouteHandler
    {
        private readonly Microsoft.Extensions.Logging.ILogger<Jasper.Configuration.IChain> _logger;

        public Jasper_Testing_Samples_ClockedEndpoint_get_clocked(Microsoft.Extensions.Logging.ILogger<Jasper.Configuration.IChain> logger)
        {
            _logger = logger;
        }



        public override Task Handle(Microsoft.AspNetCore.Http.HttpContext httpContext, System.String[] segments)
        {
            var clockedEndpoint = new Jasper.Testing.Samples.ClockedEndpoint();
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            try
            {
                var result_of_get_clocked = clockedEndpoint.get_clocked();
                return WriteText(result_of_get_clocked, httpContext.Response);
            }

            finally
            {
                stopwatch.Stop();
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, "Route 'GET: clocked' ran in " + stopwatch.ElapsedMilliseconds);)
            }

        }

    }
```

`ModifyChainAttribute` is a generic way to add middleware or post processing frames, but if you need to configure things specific to routes or message handlers, you can also use `ModifyHandlerChainAttribute` for message handlers or `ModifyRouteAttribute` for http routes.


## Policies

::: tip warning
Again, please go easy with this feature and try not to shoot yourself in the foot by getting too aggressive with custom policies
:::

You can register user-defined policies that apply to all chains or some subset of chains. For message handlers, implement this interface:

snippet: sample_IHandlerPolicy

Here's a simple sample that registers middleware on each handler chain:

snippet: sample_WrapWithSimple

Then register your custom `IHandlerPolicy` with a Jasper application like this:

snippet: sample_AppWithHandlerPolicy

## Using Configure(chain) Methods

::: tip warning
This feature is experimental, but is meant to provide an easy way to apply middleware or other configuration to specific HTTP endpoints or
message handlers without writing custom policies or having to resort to all new attributes.
:::

There's one last option for configuring chains by a naming convention. If you want to configure the chains from just one handler or endpoint class,
you can implement a method with one of these signatures:

```
public static void Configure(IChain)
{
    // gets called for each endpoint or message handling method
    // on just this class
}

public static void Configure(RouteChain chain)`
{
    // gets called for each endpoint method on this class
}

public static void Configure(HandlerChain chain)
{
    // gets called for each message handling method
    // on just this class
}
```

Here's an example of this being used from Jasper's test suite:

snippet: sample_customized_handler_using_Configure




