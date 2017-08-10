<!--title:Jasper in Console Applications-->

At this time, the Jasper team is focused on hosting applications either in IIS (or nginx) or as a console application that would be suitable for
running in a Docker container. To that end, we've added the `JasperAgent` static class as a helper for quickly standing up Jasper applications in a 
console application.

The sample usage from the <[linkto:documentation/getting_started;title=getting started]> topic would look like this:

<[sample:QuickStartConsoleMain]>

At runtime, `JasperAgent` uses the `JasperRegistry` you hand it to <[linkto:documentation/bootstrapping;title=bootstrap a JasperRuntime]> and run the application until the console process is stopped.

You can also use the command line arguments to customize how the application runs like this:

<[sample:JasperAgent-programmatic-customization]>

Or like this:

<[sample:JasperAgent-programmatic-construction]>