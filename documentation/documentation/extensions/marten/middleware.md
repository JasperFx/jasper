<!--title:Transactional Middleware-->

You can explicitly apply transactional middleware to a message or HTTP handler action with the
`[MartenTransaction]` attribute as shown below:

<[sample:CreateDocCommandHandler]>

Doing this will simply insert a call to `IDocumentSession.SaveChangesAsync()` after the last handler action is called within the generated `MessageHandler`. This effectively makes a unit of work out of all the actions that might be called to process a single message.

This attribute can appear on either the handler class that will apply to all the actions on that class, or on a specific action method.

If so desired, you *can* also use a policy to apply the Marten transaction semantics with a policy. As an example, let's say that you want every message handler where the message type
name ends with "Command" to use the Marten transaction middleware. You could accomplish that
with a handler policy like this:

<[sample:CommandsAreTransactional]>

Then add the policy to your application like this:

<[sample:Using-CommandsAreTransactional]>

## Customizing How the Session is Created

By default, using `[MartenTransaction]` or just injecting an `IDocumentSession` with the Marten integration will create a lightweight session in Marten using the `IDocumentStore.LightweightSession()`
call. However, [Marten](http://jasperfx.github.io/marten) has many other options to create sessions
with different transaction levels, heavier identity map behavior, or by attaching custom listeners. To allow you to use the full range of Marten behavior, you can choose to override the mechanics of how
a session is opened for any given message handler by just placing a method called `OpenSession()` on 
your handler class that returns an `IDocumentSession`. If Jasper sees that method exists, it will call that method to create your session. 

Here's an example from the tests:

<[sample:custom-marten-session-creation]>