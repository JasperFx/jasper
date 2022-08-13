using Jasper.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.TestSupport.Storyteller;

public static class JasperStorytellerHost
{
}

public interface INodes
{
    ExternalNode NodeFor(string serviceName);
}

public interface IJasperContext
{
    ExternalNode NodeFor(string nodeName);
}

public class ExternalNode
{
    private readonly JasperOptions _options;

    public ExternalNode(JasperOptions options)
    {
        _options = options;
    }

    public IHost Host { get; private set; }

    internal void Bootstrap(IMessageLogger logger)
    {
        _options.Services.AddSingleton(logger);
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().UseJasper(_options).Start();
    }

    internal void Teardown()
    {
        Host?.Dispose();
    }
}
