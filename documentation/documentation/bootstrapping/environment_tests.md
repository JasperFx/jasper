<!--title:Environment Tests-->

Jasper supports the idea of registerable [environment tests](http://codebetter.com/jeremymiller/2006/04/06/environment-tests-and-self-diagnosing-configuration-with-structuremap/) that you can use to verify an application is in a usable state as part of bootstrapping. The goal here is to [fail fast](https://en.wikipedia.org/wiki/Fail-fast) with environmental errors diagnosed as part of deployment.

What kind of things might you assert?

* Can your system connect to a designated database or external system?
* Is all the required configuration available?
* Are expected files where the system expects them to be?

Environment checks can be registered as lambda's or as implementations of `IEnvironmentCheck`:

<[sample:registering-environment-checks]>

Do note that Jasper is just finding every possible service registered in the system's IoC container that implements
the `IEnvironmentCheck` interface and executes each check. All environment checks will be executed when a Jasper system is
bootstrapped, and will throw a single aggregate exception with any failures. 

## Custom Environment Checks

Environment checks just need to implement this interface:

<[sample:IEnvironmentCheck]>

Like this sample from Jasper itself:

<[sample:FileExistsCheck]>

## Ignoring Startup Issues

If you so choose, you can allow a Jasper application to successfully start up even with detected
environment test failures with this configuration:

<[sample:IgnoreValidationErrors]>