using System;
using System.Collections.Generic;
using System.Linq;
using JasperBus.Model;
using Jasper.Remotes.Messaging;

namespace Jasper.Diagnostics.Messages
{
    public class DiagnosticsListener : IListener<RequestInitialData>
    {
        private readonly IDiagnosticsClient _client;
        private readonly HandlerGraph _graph;

        public DiagnosticsListener(
            IDiagnosticsClient client,
            HandlerGraph graph)
        {
            _client = client;
            _graph = graph;
        }

        public void Receive(RequestInitialData message)
        {
            var chains = _graph.Chains.OrderBy(c => c.TypeName).Select(c => ChainModel.For(c));
            _client.Send(new InitialData(chains));
        }
    }

    public class RequestInitialData : ClientMessage
    {
        public RequestInitialData()
            : base("request-initial-data")
        {
        }
    }

    public class InitialData : ClientMessage
    {
        public InitialData(IEnumerable<ChainModel> chains)
            : base("initial-data")
        {
            Chains = chains.ToArray();
        }

        public ChainModel[] Chains { get; }
    }

    public class ChainModel
    {
        public MessageTypeModel MessageType { get; set; }
        public string GeneratedTypeName { get; set; }
        public string Description { get; set; }
        public string SourceCode { get; set; }

        public static ChainModel For(HandlerChain chain)
        {
            return new ChainModel
            {
                MessageType = new MessageTypeModel(chain.MessageType),
                GeneratedTypeName = chain.TypeName,
                Description = chain.ToString(),
                SourceCode = chain.SourceCode
            };
        }
    }

    public class MessageTypeModel
    {
        public MessageTypeModel(Type type)
        {
            Name = type?.Name;
            FullName = type?.FullName;
        }

        public string Name { get; }
        public string FullName { get; }
    }
}
