Jasper
======

[![Join the chat at https://gitter.im/JasperFx/jasper](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/JasperFx/jasper?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Build status](https://ci.appveyor.com/api/projects/status/o23fp3diks7024x9?svg=true)](https://ci.appveyor.com/project/jasper-ci/jasper)


The [documentation is published here](http://jasperfx.github.io/documentation).

Jasper is a next generation application development framework for distributed server side development in .Net. At the moment, Jasper can be used as:

1. An in-memory command runner 
1. A robust, but lightweight asynchronous messaging framework (call it a service bus if you have to, but know that there's no centralized broker)
1. An alternative for authoring HTTP services within ASP.Net Core
1. A dessert topping (just kidding)

In all cases, Jasper can be used by itself or as an addon to an ASP.Net Core application. As much as possible, Jasper tries to leverage existing ASP.Net Core infrastructure.


## Working with the Code

The main solution file is `Jasper.sln`, and you should be good to go to simply open the code up in Rider, Visual Studio.Net, or VS Code and just go. In its current form, all the integration tests, including the Storyteller specifications, require [Docker](https://www.docker.com/) to be running on your development machine. For the docker dependencies (Postgresql, Rabbit MQ, Sql Server, etc.), run:

```bash
docker compose up -d
```


## What's with the name?

I think that FubuMVC turned some people off by its name ("for us, by us"). This time around I was going for an
unassuming name that was easy to remember and just named it after my (Jeremy) hometown (Jasper, MO).





