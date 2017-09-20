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