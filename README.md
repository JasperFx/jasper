Jasper
======

[![Join the chat at https://gitter.im/JasperFx/jasper](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/JasperFx/jasper?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Build status](https://ci.appveyor.com/api/projects/status/o23fp3diks7024x9?svg=true)](https://ci.appveyor.com/project/jasper-ci/jasper)


The [documentation is published here](http://jasperfx.github.io/documentation).

Jasper is a next generation application development framework for distributed server side development in .Net. At the moment, Jasper can be used as:

1. A robust, but lightweight asynchronous messaging framework (call it a service bus if you have to, but know that there's no centralized broker)
1. An in-memory command runner 
1. An alternative for authoring HTTP services within ASP.Net Core
1. A dessert topping (just kidding)

In all cases, Jasper can be used by itself or as an addon to an ASP.Net Core application. As much as possible, Jasper tries to leverage existing ASP.Net Core infrastructure.


Jasper is being built on the CoreCLR as a replacement for a small subset of the older [FubuMVC](https://fubumvc.github.io) tooling. Roughly stated, Jasper
intends to keep the things that have been successful in FubuMVC, ditch the things that weren't, and make the runtime pipeline
be much more performant. Oh, and make the stacktraces from failures within the runtime pipeline be a whole lot simpler to read -- and yes, that's absolutely worth being one of the main goals.

## Working with the Code

The *official* build script for Jasper uses Rake, but you're perfectly able to just fire up Visual Studio.Net, Rider, or VS Code and start working. In its current form, all the integration tests, including the Storyteller specifications, require [Docker](https://www.docker.com/) to be running on your developement machine.

To run the rake script, you'll need to have Ruby installed and [Docker]() running on your application. Assuming you have that, go to the command line and type:

1. `bundle install` (only the first time)
1. `rake` or if you have version conflicts with other projects on your machine (boo), use `bundle exec rake`

This script will build the client side Javascript assets, restore .Net dependencies, execute the xUnit tests, and run the [Storyteller](http://storyteller.github.io) specifications.

## Working with the Integration Tests

If you use the Rake script, you can execute the `IntegrationTests` library directly by using:

```
rake integrationtests
```

If you don't want to use Rake, from a command line you need to run this at least once before executing any of the `IntegrationTests`:

```
docker-compose up -d
```


## Working with the Storyteller Specifications

See the section on `IntegrationTests` above for directions on using Docker.

If you're okay using the rake script, just use the `rake open_st` task to start and launch the Storyteller specification editor. `rake storyteller` will run the specifications from a command line if you only need to see results. The command `rake storyteller` will likewise run the specification suite from the command line.

If you don't want to use rake, go to the `src/StorytellerSpecs` folder at the command line and type `dotnet storyteller`.


## Documentation

The documentation is built and published with [dotnet stdocs](http://storyteller.github.io/documentation/docs/). The actual content is
in this repository in the "/documentation" folder, but the finished HTML docs will be published to gh-pages in the
[jasperfx.github.io](https://github.com/JasperFx/jasperfx.github.io) repository.

To run the documentation website for the Jasper docs, use the `rake docs` task, or if you're not a Ruby fan, from the
root directory of the repository, do `dotnet restore && dotnet stdocs run`.

To publish the documentation, there is a separate `rake publish` task that exports the compiled HTML code and pushes that to the Github
repository for Jasper. Note that you will have to have push rights to the jasperfx.github.io repository.


## Running Benchmarks

There is some rudimentary benchmarks exposed through [BenchmarkDotNet](http://benchmarkdotnet.org/) in the `src/benchmarks` project. To run the benchmarks, go to the command line at the `src/benchmarks` folder and just run `dotnet run -c Release` to execute all of the benchmarks.


## What's with the name?

I think that FubuMVC turned some people off by its name ("for us, by us"). This time around I was going for an
unassuming name that was easy to remember and just named it after my (Jeremy) hometown (Jasper, MO).





