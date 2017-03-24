# Jasper Diagnostics

1. Add the Diagnostic Services to your `JasperRegistry`

```csharp
public class BusRegistry : JasperBusRegistry
{
    public BusRegistry()
    {
        ...

        this.AddDiagnostics();
    }
}
```

2.  Add the Diagnostics endpoint to your application configuration

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    app.UseDiagnostics();
}
```
