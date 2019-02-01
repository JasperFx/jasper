using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Http.Model;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Jasper.Testing.Util;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Testing.Samples
{
    // SAMPLE: StopwatchFrame
    public class StopwatchFrame : SyncFrame
    {
        private readonly IChain _chain;
        private Variable _logger;
        private readonly Variable _stopwatch;

        public StopwatchFrame(IChain chain)
        {
            _chain = chain;

            // This frame creates a Stopwatch, so we
            // expose that fact to the rest of the generated method
            // just in case someone else wants that
            _stopwatch = new Variable(typeof(Stopwatch), "stopwatch", this);
        }


        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var stopwatch = new {typeof(Stopwatch).FullNameInCode()}();");
            writer.Write($"stopwatch.Start();");

            writer.Write("BLOCK:try");
            Next?.GenerateCode(method, writer);
            writer.FinishBlock();

            // Write a finally block where you record the stopwatch
            writer.Write("BLOCK:finally");

            writer.Write($"stopwatch.Stop();");
            writer.Write($"{_logger.Usage}.Log(Microsoft.Extensions.Logging.LogLevel.Information, \"{_chain.Description} ran in \" + {_stopwatch.Usage}.{nameof(Stopwatch.ElapsedMilliseconds)});)");

            writer.FinishBlock();
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            // This in effect turns into "I need ILogger<IChain> injected into the
            // compiled class"
            _logger = chain.FindVariable(typeof(ILogger<IChain>));
            yield return _logger;
        }
    }
    // ENDSAMPLE

    // SAMPLE: StopwatchAttribute
    public class StopwatchAttribute : ModifyChainAttribute
    {
        public override void Modify(IChain chain, JasperGenerationRules rules)
        {
            chain.Middleware.Add(new StopwatchFrame(chain));
        }
    }
    // ENDSAMPLE

    // SAMPLE: ClockedEndpoint
    public class ClockedEndpoint
    {
        [Stopwatch]
        public string get_clocked()
        {
            return "how fast";
        }
    }
    // ENDSAMPLE


    // SAMPLE: using-IRoutePolicy-and-IHandlerPolicy
    public class PutStopwatchOnRoutes : IRoutePolicy
    {
        public void Apply(RouteGraph graph, JasperGenerationRules rules)
        {
            // Put this middleware on any route that
            // has the HTTP method POST, PUT, or DELETE
            foreach (var chain in graph.Commands)
            {
                chain.Middleware.Add(new StopwatchFrame(chain));
            }
        }
    }

    public class PutStopwatchOnHandlers : IHandlerPolicy
    {
        public void Apply(HandlerGraph graph, JasperGenerationRules rules)
        {
            // We're adding the StopwatchFrame to all message types,
            // but we *could* filter the application
            foreach (var chain in graph.Chains)
            {
                chain.Middleware.Add(new StopwatchFrame(chain));
            }
        }
    }

    public class StopwatchMonitoredApp : JasperRegistry
    {
        public StopwatchMonitoredApp()
        {
            // Apply a handler policy
            Handlers.GlobalPolicy<PutStopwatchOnHandlers>();

            HttpRoutes.GlobalPolicy<PutStopwatchOnRoutes>();
        }
    }
    // ENDSAMPLE

    public class try_it_out
    {
        private readonly ITestOutputHelper _output;

        public try_it_out(ITestOutputHelper output)
        {
            _output = output;
        }

        public void what_we_are_trying_to_do(ILogger<IChain> logger)
        {
            // SAMPLE: stopwatch-concept
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                // execute the HTTP request
                // or message
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation("Ran something in " + stopwatch.ElapsedMilliseconds);
            }
            // ENDSAMPLE
        }

        [Fact]
        public void generate_code()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery();
            registry.HttpRoutes.DisableConventionalDiscovery();
            registry.HttpRoutes.IncludeType<ClockedEndpoint>();

            using (var runtime = JasperHost.For(registry))
            {
                var route = runtime.Get<RouteGraph>().Single();
                var code = route.SourceCode;

                _output.WriteLine(code);

            }
        }
    }
}
