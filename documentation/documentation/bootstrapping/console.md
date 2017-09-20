<!--title:Jasper in Console Applications-->

At this time, the Jasper team is focused on hosting applications either in IIS (or nginx) or as a console application that would be suitable for
running in a Docker container. To that end, we've added the `JasperAgent` static class in the 
external `Jasper.CommandLine` Nuget library as a helper for quickly standing up Jasper applications in a console application.

The sample usage from the <[linkto:documentation/getting_started;title=getting started]> topic would look like this:

<[sample:QuickStartConsoleMain]>

At runtime, `JasperAgent` uses the `JasperRegistry` you hand it to <[linkto:documentation/bootstrapping;title=bootstrap a JasperRuntime]> and run the application until the console process is stopped.

You can also use the command line arguments to customize how the application runs like this:

<[sample:JasperAgent-programmatic-customization]>

Or like this:

<[sample:JasperAgent-programmatic-construction]>

Let's say that your Jasper application compiles to `MyApp.exe` and uses the `JasperAgent` class
to run the commands. In that case you can run your application simply by typing `MyApp` at the
command line with no arguments.

However, the `Jasper.CommandLine` library adds some additional commands for running, validating, or describing the running application.


## Overriding the Environment Name

For example, you can also use this syntax to run your application in "Development" mode:

```
MyApp run --environment Development
```

or 

```
MyApp run -e Development
```

If you run this command, your application will start with `JasperRegistry.EnvironmentName` equal to _Development_. If you programmatically set the environment name in your `JasperRegistry`, that
setting will win out over the command line flag.

## Running with Verbose Console Tracing

Likewise, to see more verbose information on start up and runtime console tracing, use:

```
MyApp run --verbose
```

or 

```
MyApp run -v
```

## Validating the Configured Application

You may want to simply try to bootstrap the application and run all the <[linkto:documentation/bootstrapping/environment_tests;title=environment tests]> and report out the results. That syntax is:

```
MyApp validate
```

which also respects the same `--environment` and `--verbose` flags as the run command. This command will bootstrap the application, run all the environment tests and start up validations, report on the success or failure, and shut down the application. Do note that if any environment tests fail, this command will return a non-zero return code that should be sufficient to let any
build scripting tool you're using know that the validation failed.

## List Registered Services

**This is in flight**

As of right now (Sept 2017), Jasper only supports the [StructureMap](http://structuremap.github.io) container, but is likely to transition to just using the built in
ASP.Net Core IoC container before it hits 1.0. Regardless, we expect this command will live on.

To bootstrap the application and list out all the services registered to the system's IoC container, use this command:

```
MyApp services
```

And again, this command respects both the `--environment` and `--verbose` flags

## Subscriptions

See <[linkto:documentation/messaging/routing/subscriptions]> for information about the `subscriptions` command and related workflow for exporting, updating, or validating dynamic
subscriptions.

