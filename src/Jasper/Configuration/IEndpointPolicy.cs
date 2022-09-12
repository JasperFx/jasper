using System;
using Jasper.Runtime;

namespace Jasper.Configuration;

public interface IEndpointPolicy
{
    void Apply(Endpoint endpoint, IJasperRuntime runtime);
}

internal class LambdaEndpointPolicy<T> : IEndpointPolicy where T : Endpoint
{
    private readonly Action<T, IJasperRuntime> _configure;

    public LambdaEndpointPolicy(Action<T, IJasperRuntime> configure)
    {
        _configure = configure;
    }

    public void Apply(Endpoint endpoint, IJasperRuntime runtime)
    {
        if (endpoint is T e) _configure(e, runtime);
    }
}
