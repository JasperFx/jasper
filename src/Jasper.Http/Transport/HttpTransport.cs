using System;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
using Baseline.Dates;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Jasper.Http.Transport
{
    public class HttpTransport : ITransport
    {
        private readonly BusSettings _settings;
        private readonly HttpTransportSettings _httpSettings;
        private readonly JasperRuntime _runtime;
        private readonly IPersistence _persistence;
        private readonly CompositeTransportLogger _logger;

        public HttpTransport(BusSettings settings, HttpTransportSettings httpSettings, JasperRuntime runtime, IPersistence persistence, CompositeTransportLogger logger)
        {
            _settings = settings;
            _httpSettings = httpSettings;
            _runtime = runtime;
            _persistence = persistence;
            _logger = logger;
        }


        public void Dispose()
        {

        }

        public string Protocol { get; } = "http";

        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            var batchedSender = new BatchedSender(uri, new HttpSenderProtocol(_settings, _httpSettings), cancellation, _logger);

            ISendingAgent agent;

            if (uri.IsDurable())
            {
                agent = _persistence.BuildSendingAgent(uri, batchedSender, cancellation);
            }
            else
            {
                agent = new LightweightSendingAgent(uri, batchedSender, _logger, _settings);
            }

            agent.DefaultReplyUri = LocalReplyUri;
            agent.Start();

            return agent;
        }

        // The sending agent will take care of this one
        public Uri LocalReplyUri { get; private set; }

        public void StartListening(BusSettings settings, IWorkerQueue workers)
        {
            if (_runtime.HttpAddresses.IsNotEmpty())
            {
                var candidate = _runtime.HttpAddresses.Split(';').Select(x => x.ToUri()).FirstOrDefault(x => x.Host == "localhost" || x.Host == "127.0.0.1")?.ToMachineUri();
                if (candidate != null)
                {
                    LocalReplyUri = candidate.ToString().TrimEnd('/').AppendUrl(_httpSettings.RelativeUrl).ToUri();
                }
            }

        }

        public void Describe(TextWriter writer)
        {
            if (_httpSettings.EnableMessageTransport)
            {
                writer.WriteLine($"Listening for messages at {LocalReplyUri}");
                writer.WriteLine($"Listening for messages at {LocalReplyUri.ToString().AppendUrl("durable")}");
            }
        }
    }
}
