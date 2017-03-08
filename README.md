Jasper
======

[![Join the chat at https://gitter.im/JasperFx/jasper](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/JasperFx/jasper?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Jasper is a next generation application development framework for distributed server side development in .Net. Jasper is being built on the CoreCLR as a replacement for a small subset of the older [FubuMVC](https://fubumvc.github.io) tooling. Roughly stated, Jasper
intends to keep the things that have been successful in FubuMVC, ditch the things that weren't, and make the runtime pipeline
be much more performant. Oh, and make the stacktraces from failures within the runtime pipeline be a whole lot simpler to read -- and yes, that's absolutely worth being one of the main goals.


The current thinking is that we'd have these libraries/Nugets:

1. _Jasper_ - The core assembly that will handle bootstrapping, configuration, and the Roslyn code generation tooling
1. _JasperBus_ - The service bus features from FubuMVC and an alternative to [MediatR](https://github.com/jbogard/MediatR)
1. _JasperDiagnostics_ - Runtime diagnostics meant for development and testing
1. _JasperStoryteller_ - Support for hosting Jasper applications within 
   [Storyteller](http://storyteller.github.io) specification projects. 
1. _JasperHttp_ (later) - Build HTTP micro-services on top of ASP.Net Core in a FubuMVC-esque way.
1. _JasperQueues_ (later) - JasperBus is going to use [LightningQueues](https://github.com/LightningQueues/LightningQueues) as its   
   primary transport mechanism, but I'd like to re-architect that code to a new library inside of Jasper. This library will 
   not have any references or coupling to any other Jasper project.
1. _JasperScheduler_ (proposed for much later) - Scheduled or polling job support on top of JasperBus

## The Core Pipeline and Roslyn

The basic goal of Jasper is to provide a much more efficient and improved version of the older FubuMVC architecture for
CoreCLR development that is also "wire compatible" with our existing FubuMVC 3 services on .Net 4.6.

The original, core concept of FubuMVC was what we called the [Russion Doll Model](http://codebetter.com/jeremymiller/2011/01/09/fubumvcs-internal-runtime-the-russian-doll-model-and-how-it-compares-to-asp-net-mvc-and-openrasta/) and is now mostly refered to as _middleware_. The _Russian Doll Model_ architecture makes it relatively easy for developers to reuse code for cross cutting concerns
like validation or security without having to write nearly so much explicit code. At this point, many other .Net frameworks support
some kind of _Russian Doll Model_ architecture like [ASP.Net Core's middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware) or the [Behavior model](https://docs.particular.net/nservicebus/pipeline/manipulate-with-behaviors) in [NServiceBus](https://particular.net/nservicebus).


In FubuMVC, that consisted of a couple parts:

* A runtime abstraction for middleware called `IActionBehavior` for every step in the runtime pipeline for processing an HTTP request   or service bus message. Behavior's were a linked list chain from outermost behavior to innermost. This model was also adapted from    FubuMVC into [NServiceBus](https://particular.net/nservicebus).

* A configuration time model we called the `BehaviorGraph` that expressed all the routes and service bus message handling chains of 
  behaviors in the system. This configuration time model made it possible to apply conventions and policies that established
  what exact middleware ran in what order for each message type or HTTP route. This configuration model also allowed FubuMVC to 
  expose diagnostic visualizations about each chain that was valuable for troubleshooting problems or just flat out understanding
  what was in the system to begin with.

Great, lots of flexibility and some unusual diagnostics, but the FubuMVC model gets a lot uglier when you go to an "async by default" execution pipeline. Maybe more importantly, it suffers from too many object allocations because of all the little objects getting created on every message or HTTP request. Lastly, it makes for some truly awful stacktraces when things go wrong because of all the bouncing between behaviors in the nested handler chain.

For Jasper, we're going to keep the configuration model (but simplified from FubuMVC), but this time around
we're doing some code generation at runtime to "bake" the execution pipeline in a much tighter package, then use the 
new [runtime code compilation capabilitites in Roslyn](https://jeremydmiller.com/2015/11/11/using-roslyn-for-runtime-code-generation-in-marten/) to generate assemblies on the fly.

As part of that, we're trying every possible trick we can think of to reduce object allocations and minimize the work being done
at runtime by the underlying IoC container.


## What's with the name?

I think that FubuMVC turned some people off by its name ("for us, by us"). This time around I was going for an
unassuming name that was easy to remember and just named it after my hometown (Jasper, MO). 


## JasperBus

The initial feature set looks to be:

* Running decoupled commands ala MediatR
* In memory transport
* LightningQueues based transport
* Publish/Subscribe messaging
* Request/Reply messaging patterns
* Dead letter queue mechanics
* Configurable error handling rules 
* The ["cascading messages" feature from FubuMVC](https://fubumvc.github.io/documentation/servicebus/cascading/)
* Static message routing rules
* Subscriptions for dynamic routing -- this time we're looking at using [Consul(https://www.consul.io/)] for the underlying storage
* [Delayed messages](https://fubumvc.github.io/documentation/servicebus/delayed/)
* Batch message processing
* Saga support (later) -- but this is going to be a complete rewrite from FubuMVC

There is no intention to add the polling or scheduled job functionality that was in FubuMVC to Jasper.


## JasperDiagnostics

We haven't detailed this one out much, but I'm thinking it's going to be a completely encapsulated ASP.Net Core
application using Kestrel to serve some diagnostic views of a running Jasper application. As much as anything,
I think this project is going to be a test bed for my shop's approach to React/Redux and an excuse to experiment
with the [Apollo](http://dev.apollodata.com/react/) client with or without GraphQL. The diagnostics should expose
both a static view of the application's configuration and a live tracing of messages or HTTP requests being handled.


## JasperStoryteller

This library won't do too much, but we'll at least want a recipe for being able to bootstrap and teardown a Jasper
application in Storyteller test harnesses. At a minimum, I'd like to expose a bit of diagnostics on the service
bus activity during a Storyteller specification run [like we did with FubuMVC](https://jeremydmiller.com/2016/05/17/reliable-and-debuggable-automated-testing-of-message-based-systems-in-a-crazy-async-world/)
in the Storyteller specification results HTML.


## JasperHttp

We're embracing ASP.net Core MVC at work, so this might just be a side project for fun down the road. The goal here is just to 
provide a mechanism for writing micro-services that expose HTTP endpoints. The  I think the potential benefits over MVC are:

* Less ceremony in writing HTTP endpoints (fewer attributes, no required base classes, no marker interfaces, no fluent interfaces)
* The runtime model will be much leaner. We **think** that we can make Jasper about as efficient as writing purely explicit, bespoke
  code directly on top of ASP.Net Core
* Easier testability


## JasperScheduler

If necessary, we'll have another "Feature" library that extends JasperBus with the ability
to schedule user supplied jobs. The intention this time around is to just use [Quartz](https://www.quartz-scheduler.net/) as the actual scheduler. 


## JasperQueues

This is a giant TBD


## IoC Usage Plans

Right now, it's going to be [StructureMap](http://structuremap.github.io) 4.4+ only. While this will drive some folks away,
it makes the tool much easier to build. Besides, Jasper is already using some StructureMap functionality for its own configuration.
I think that we're only positioning Jasper for greenfield projects (and migration from FubuMVC) anyway.

Regardless, the IoC usage in Jasper is going to be simplistic compared to what we did in FubuMVC and certainly less entailed 
than the IoC abstractions in ASP.net MVC Core. We theorize that this should make it possible to slip in the IoC container of
your choice later.


