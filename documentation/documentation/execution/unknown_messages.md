<!--title:Handling an Unknown Message-->

In the case where your system receives a message with a message type that you cannot handle,
Jasper gives you the ability to write your own policy about how to deal with that message with the
`IMissingHandler` interface shown below:

snippet: sample_IMissingHandler

To create your own policy or fall through handler, implement your own version of that
interface like this sample shown below:

snippet: sample_MyMissingHandler

Lastly, to register your own policy, just add it to the application IoC container like so:

snippet: sample_ConfigureMissingHandler

Do note that you can add multiple `IMissingHandler`'s in a single application and all of them will
be executed.
