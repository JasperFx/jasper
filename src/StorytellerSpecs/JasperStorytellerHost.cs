using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Logging;
using Jasper.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StoryTeller;
using StoryTeller.Engine;
using StorytellerSpecs.Logging;

namespace Jasper.TestSupport.Storyteller
{
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
}
