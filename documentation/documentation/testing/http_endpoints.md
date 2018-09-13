<!--title:Testing HTTP Endpoints with Alba-->

The `Jasper.TestSupport.Alba` Nuget library adds the ability to write [Alba HTTP contract tests](https://jasperfx.github.io/alba) against a Jasper application's HTTP endpoints. 

Assume that you have a simple HTTP endpoint in your system like this one shown below that just responds to the "GET: /salutations" Url by
writing text to the HTTP response:

<[sample:GreetingsEndpoint]>

An Alba specification might look like this:

<[sample:AlbaScenarioUsage]>

In real usage though, you'd probably want to reuse your `JasperRuntime` object between tests because that object can be a little expenseive timewise to construct, and you almost always want your test suite to run as quickly as possible for faster feedback. If you were using [xUnit.Net](https://xunit.github.io/), you might take an approach where you use a shared fixture for the `JasperRuntime` like this:

<[sample:AlbaScenarioUsageShared]>







