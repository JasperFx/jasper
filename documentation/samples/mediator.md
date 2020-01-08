<!--title:Jasper as an In Memory Command Executor / Mediator-->

<[info]>
While Jasper can certainly play the same role that MediatR does today for many ASP.Net Core developers,
the Jasper team suggests in that case that you completely bypass MVC Core and just use Jasper HTTP endpoints as
that will be more efficient.
<[/info]>

With the popularity of [MediatR](https://github.com/jbogard/MediatR) as an add on to ASP.Net Core to be an in memory command execution "bus," 
there's clearly some demand for that kind of tooling within ASP.Net MVC applications (or within any kind of headless .Net Core application). Jasper
can also fit the bill as an in memory command executor that:

* Decouples your MVC Core controllers from any other services
* Allows for dealing with cross cutting concerns with a *Russian Doll* middleware strategy
* Can execute the command immediately
* Enqueue the command in local working queues
* Schedule the command for later execution
* Locally "store and forward" the command before enqueueing it to ensure that the local enqueue process is durable
* Is very efficient with runtime resources compared to other command execution tools in .Net
* Won't make you pollute your code or go down crazy rabbit holes with .Net generics 


<[info]>
Jasper supports a **superset** of MediatR's functionality, so don't just do an apples to apples comparison here please;)
<[/info]> 

As an example, let's say that your application needs to accept and execute a command called `RegisterUser` -- an example shamelessly stolen from [this post from Jon Hilton](https://jonhilton.net/2016/06/06/simplify-your-controllers-with-the-command-pattern-and-mediatr/). Assuming that you have the skeleton of an <[linkto:tutorials/mvc;title=MVC Core application with Jasper installed]>, the first step is to write a message handler as shown in the next section.

## Writing a Command Handler

Let's say that your system is using [EF Core](https://docs.microsoft.com/en-us/ef/core/), and eventually your new user will be persisted with an EF Core `DbContext`. The shell of your command handler for `RegisterUser` might look like this:

<[sample:RegisterUserHandler]>

That's it. No interfaces, no generic arguments, and our code is just that, code. Behind the scenes, Jasper itself generates and compiles code at runtime
to wrap its own `MessageHandler` around yours, along with whatever service activation, service clean up, and applied middleware into a single class that acts as an adapter between Jasper and your message handler method (*this is contrived if you're wondering where the configuration is for the UserDbContext*):

```
    public class MvcCoreHybrid_Controllers_RegisterUser : Jasper.Messaging.Model.MessageHandler
    {
        public override async Task Handle(Jasper.Messaging.IMessageContext context)
        {
            var registerUserHandler = new MvcCoreHybrid.Controllers.RegisterUserHandler();
            using (var userDbContext = new MvcCoreHybrid.UserDbContext())
            {
                var registerUser = (MvcCoreHybrid.Controllers.RegisterUser)context.Envelope.Message;
                var userRegistered = await registerUserHandler.Handle(registerUser, userDbContext);
                await context.Advanced.EnqueueCascading(userRegistered);
            }
        }
    }

```

Next, you can execute the `RegisterUser` command from within an MVC Core `Controller` method like so:

<[sample:UserController]>

Where `ICommandBus` is a slimmed down interface inside of Jasper that exposes all the local command execution capabilities
of the larger `IMessageContext` shown in the rest of these docs.


You can read much more about <[linkto:handling/discovery;title=how message handlers are discovered here]>, and more options for building message handlers in <[linkto:handling/handlers]>. 

## What else can Jasper do?

Quite a bit! Here are some examples of the other things Jasper can do as an in-memory command executor:

<[sample:what-else-can-the-in-memory-bus-do]>

You can learn much more at <[linkto:messaging]>