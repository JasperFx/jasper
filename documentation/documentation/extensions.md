<!--Title:Extensions-->

Jasper comes with its own extension system heavily influenced by its [FubuMVC](https://fubumvc.github.io/) predecessor. The basic concept is just based on the 
`IJasperExtension` shown below:

snippet: sample_IJasperExtension

So, what can you do in an extension? Basically anything you can configure in a Jasper application
can be altered by an `IJasperExtension`. Here's a contrived example:

snippet: sample_SampleExtension

Do note that in the case of service registrations and any kind of `Settings.Apply()` calls, the extension alterations are applied first before any registrations or declarations in the main
`JasperRegistry`. What this means in effect is just that the application configuration takes precedence over extensions.

Now, on to how you would consume extensions in your projects:

## Explicitly Loading Extensions

You can also explicitly create and consume `IJasperExtension` classes that are not automatically loaded through the `[JasperModule]` attribute that are be applied using the `JasperRegistry.Include()` method shown below:

snippet: sample_explicitly_add_extension

## Auto Discovery of Extensions

When you build an extension, you also have the option to make a single extension be auto-discoverable from an external assembly. To make that concrete, here's an auto-discovered
extension in the Jasper.Marten library that wires up integration with a [Marten](https://jasperfx.github.io/marten) database:

snippet: sample_MartenExtension

When a Jasper application is being bootstrapped, it looks for any assemblies in the application's bin directory that are marked with the `[JasperModule]` attribute to determine the extension types that should be automatically applied to the application. In the case above, the
Marten extension would be discovered at bootstrapping time and the `MartenExtension` would be
applied to the application. 

## Importing Http Endpoints or Message Handlers

It's perfectly possible to add additional HTTP routes or message handlers in extensions using the explicit `JasperRegistry.Handlers.IncludeType()` or `JasperRegistry.Http.Actions.IncludeType()` methods.



