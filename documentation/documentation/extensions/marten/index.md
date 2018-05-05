<!--title:Jasper.Marten-->

The Jasper.Marten library provides some easy to use recipes for integrating  [Marten](https://jasperfx.github.io/marten) and Postgresql into a Jasper application. All you need to do to get
started with Marten + Jasper is to add the *Jasper.Marten* nuget to your project and at minimum,
at least set the connection string to the underlying Postgresql database by configuring
Marten's `StoreOptions` object like this:

<[sample:AppWithMarten]>

Note that `ConfigureMarten()` is an extension method in Jasper.Marten.

Once that's done, you will be able to inject the following Marten services as either constructor
arguments or method parameters in message or HTTP handlers:

1. `IDocumentStore`
1. `IDocumentSession` - opened with the default `IDocumentStore.OpenSession()` method
1. `IQuerySession`

Likewise, all of these service will be registered in the underlying IoC container for the application.

If you need to customize an `IDocumentSession` for something like transaction levels or automatic dirty checking, we recommend that you just take in `IDocumentStore` and create the session in the application code.

As an example:

<[sample:UsingDocumentSessionHandler]>

On top of the auto-discovered Marten service integration, the Jasper.Marten extension also has:

<[TableOfContents]>