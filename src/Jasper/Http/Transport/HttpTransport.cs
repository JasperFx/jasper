using System;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Jasper.Http.Transport
{
    public class HttpTransport : ITransport
    {
        private readonly MessagingSettings _settings;
        private readonly HttpTransportSettings _httpSettings;
        private readonly JasperRuntime _runtime;
        private readonly IDurableMessagingFactory _durableMessagingFactory;
        private readonly ITransportLogger _logger;

        public HttpTransport(MessagingSettings settings, JasperRuntime runtime, IDurableMessagingFactory factory, ITransportLogger logger)
        {
            _settings = settings;
            _httpSettings = settings.Http;
            _runtime = runtime;
            _durableMessagingFactory = factory;
            _logger = logger;
        }


        public void Dispose()
        {

        }

        public string Protocol { get; } = "http";

        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            var batchedSender = new BatchedSender(uri, new HttpSenderProtocol(_settings), cancellation, _logger);

            ISendingAgent agent;

            if (uri.IsDurable())
            {
                agent = _durableMessagingFactory.BuildSendingAgent(uri, batchedSender, cancellation);
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

        public void StartListening(IMessagingRoot root)
        {
            if (_runtime.HttpAddresses.Any())
            {


                var candidate = _runtime.HttpAddresses.Select(x => x.ToUri()).FirstOrDefault(x => x.Host == "localhost" || x.Host == "127.0.0.1")?.ToMachineUri();
                if (candidate != null)
                {
                    LocalReplyUri = candidate.ToString().TrimEnd('/').AppendUrl(_httpSettings.RelativeUrl).ToUri();
                }
            }

        }

        public void Describe(TextWriter writer)
        {
            if (_httpSettings.IsEnabled)
            {
                writer.WriteLine($"Listening for messages at {LocalReplyUri}");
                writer.WriteLine($"Listening for messages at {LocalReplyUri.ToString().AppendUrl("durable")}");
            }
        }
    }

}
