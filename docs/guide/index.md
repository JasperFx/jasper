# Getting Started

::: tip
Jasper targets .Net 6 and above.
:::

Jasper is a toolset for command execution and message handling within .Net Core applications.
The killer feature of Jasper (we think) is its very efficient command execution pipeline that
can be used as:

1. An inline "mediator" pipeline for executing commands
2. An in memory messaging bus and command executor within .Net applications
3. When used in conjunction with low level messaging infrastructure tools like RabbitMQ, a full fledged asynchronous messaging platform for robust communication and interaction between services

Jasper tries very hard to be a good citizen within the .Net ecosystem and even when used in
"headless" services, uses many elements of ASP.Net Core (logging, configuration, bootstrapping, hosted services)
rather than try to reinvent something new. Jasper utilizes the new .Net Generic Host for bootstrapping and application teardown.
This makes Jasper relatively easy to use in combination with many of the most popular .Net tools.

## Your First Jasper Application

TODO -- link to the GitHub sample att here

For a first application, let's say that we're building a very simple issue tracking system for
our own usage. If you're reading this web page, it's a pretty safe bet you spend quite a bit of time
working with an issue tracking system:)

Ignoring any discussion of the user interface or even a backing database, let's
start a new web api project for this new system with:

```bash
dotnet new webapi
```
Next, let's add Jasper to our project with:

```bash
dotnet add package Jasper
```

To start off, we're just going to build two API endpoints that accepts
a POST from the client that...

1. Creates a new `Issue`, stores it, and triggers an email to internal personal.
2. Assigns an `Issue` to an existing `User` and triggers an email to that user letting them know there's more work on their plate

The two *commands* for the POST endpoints are below:

snippet: sample_Quickstart_commands

To keep things dirt simple, all the issue and user storage is just in memory right now
with singleton scoped repository classes like this:

snippet: sample_Quickstart_IssueRepository

Let's jump right into the `Program.cs` file of our new web service:

snippet: sample_Quickstart_Program

Alright, let's talk about what's going on up above:

1. I integrated Jasper into the new system through the call to `IHostBuilder.UseJasper()`
2. I registered the `UserRepository` and `IssueRepository` services
3. I created a couple [Minimal API](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0) endpoints

TODO -- link to jasper as command bus

The two Web API functions directly delegate to Jasper's `ICommandBus.InvokeAsync()` method.
In that method, Jasper will direct the command to the correct handler and invoke that handler
inline. In a simplistic form, here is the entire handler file for the `CreateIssue`
command:

snippet: sample_Quickstart_CreateIssueHandler

Hopefully that code is simple enough, but let's talk what you do not see in this code or
the initial `Program` code up above.

Jasper uses a naming convention to automatically discover message handler actions in your
application assembly, so at no point did we have to explicitly register the
`CreateIssueHandler` in any way.

We didn't have to use any kind of base class, marker interface, or .Net attribute to designame
any part of the behavior of the `CreateIssueHandler` class. In the `Handle()` method, the
first argument is always assumed to be the message type for the handler action. It's not apparent
in any of the quick start samples, but Jasper message handler methods can be asynchronous as
well as synchronous, depending on what makes sense in each handler. So no littering your code
with extraneous `return Task.Completed;` code like you'd have to with other .Net tools.

As I mentioned earlier, we want our API to create an email whenever a new issue is created. In
this case I'm opting to have that email generation and email sending happen in a second
message handler that will run after the initial command. You might also notice that the `CreateIssueHandler.Handle()` method returns an `IssueCreated` event.
When Jasper sees that a handler creates what we call a [cascading message](TODO -- link here!), Jasper will
publish the `IssueCreated` event to an in memory
queue after the initial message handler succeeds. The advantage of doing this is allowing the
slower email generation and sending process to happen in background processes instead of holding up
the initial web service call.

The `IssueHandled` event message will be handled by this code:

snippet: sample_Quickstart_IssueCreatedHandler

Now, you'll notice that Jasper is happy to allow you to use static methods as
handler actions. And also notice that the `Handle()` method takes in an argument
for `IssueRepository`. Jasper always assumes that the first argument of an handler
method is the message type, but other arguments are inferred to be services from the
system's underlying IoC container. By supporting [method injection](https://betterprogramming.pub/the-3-types-of-dependency-injection-141b40d2cebc) like this, Jasper
is able to cut down on even more of the typical cruft code forced upon you by other .Net tools.

*You might be saying that this sounds like the behavior of the conventional method injection
behavior of Minimal API in .Net 6, and it is. But I'd like to point out that Jasper had this
years before the ASP.Net team got around to it:-)*

This page introduced the basic usage of Jasper, how to wire Jasper
into .Net applications, and some rudimentary `Handler` usage. There's much more
of course, so learn more with:

TODO -- add content to learn more

TODO -- link to Handler discovery
