# Bootstrapping Compliance

-> id = fc5f030f-63d0-4ace-b025-9f231291a55f
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2018-07-09T11:58:20.1796820Z
-> tags = 

[AspNetCoreIntegration]

This was originally a poorly performing xUnit suite that didn't behave in CI

|> SettingsShouldBe Team=chiefs, City=Austin, Environment=Green
|> TheEnvironmentChecksRan
|> CanHitJasperRoutes
|> CanHitAspNetCoreRoutes
|> GetsAppBuilderConfigurationFromJasperRegistryHostCalls
|> GetKey key=foo, value=bar
|> GetKey key=team, value=chiefs
|> HasHandlers
|> HasMessageActivatorBeforeOtherActivators
|> HasServiceRegistrationsFromJasper
~~~
