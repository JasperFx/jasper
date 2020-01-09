<!--title:Using Entity Framework Core with Jasper-->

The `Jasper.EntityFrameworkCore` Nuget can be used with a Jasper application to add support for the `[Transactional]` middleware, outbox support, and saga persistence using [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/). Note that you will **also** need
to use one of the database backed message persistence mechanisms like <[linkto:durability/sqlserver;title=Jasper.SqlServer]> or <[linkto:durability/postgresql;title=Jasper.Postgresql]> in conjunction with the EF Core integration.

As an example of using the EF Core integration with Sql Server, 