using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Transports;
using Jasper.Transports.Local;
using Oakton.Descriptions;
using Spectre.Console;

namespace Jasper
{
    public partial class JasperOptions : IEnumerable<ITransport>, IEndpoints, IDescribedSystemPart, IWriteToConsole, IAsyncDisposable
    {
        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();

        public ITransport? TransportForScheme(string scheme)
        {
            return _transports.TryGetValue(scheme.ToLowerInvariant(), out var transport)
                ? transport
                : null;
        }

        public void Add(ITransport transport)
        {
            foreach (var protocol in transport.Protocols)
            {
                _transports.SmartAdd(protocol, transport);
            }
        }

        public T Get<T>() where T : ITransport, new()
        {
            var transport = _transports.Values.OfType<T>().FirstOrDefault();
            if (transport == null)
            {
                transport = new T();
                foreach (var protocol in transport.Protocols)
                {
                    _transports[protocol] = transport;
                }
            }

            return transport;
        }

        public IEnumerator<ITransport> GetEnumerator()
        {
            return _transports.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Endpoint? TryGetEndpoint(Uri uri)
        {
            return findTransport(uri).TryGetEndpoint(uri);
        }

        private ITransport findTransport(Uri uri)
        {
            var transport = TransportForScheme(uri.Scheme);
            if (transport == null)
            {
                throw new InvalidOperationException($"Unknown Transport scheme '{uri.Scheme}'");
            }

            return transport;
        }

        public Endpoint GetOrCreateEndpoint(Uri uri)
        {
            return findTransport(uri).GetOrCreateEndpoint(uri);
        }


        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        public IListenerConfiguration ListenForMessagesFrom(Uri uri)
        {
            var settings = findTransport(uri).ListenTo(uri);
            return new ListenerConfiguration(settings);
        }

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        public IListenerConfiguration ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(new Uri(uriString));
        }



        public void Publish(Action<PublishingExpression> configuration)
        {
            var expression = new PublishingExpression(this);
            configuration(expression);
            expression.AttachSubscriptions();
        }

        public IPublishToExpression PublishAllMessages()
        {
            var expression = new PublishingExpression(this)
            {
                AutoAddSubscriptions = true
            };

            expression.AddSubscriptionForAllMessages();
            return expression;
        }

        public IListenerConfiguration LocalQueue(string queueName)
        {
            var settings = Get<LocalTransport>().QueueFor(queueName);
            return new ListenerConfiguration(settings);
        }

        public IListenerConfiguration DefaultLocalQueue => LocalQueue(TransportConstants.Default);
        public IListenerConfiguration DurableScheduledMessagesLocalQueue => LocalQueue(TransportConstants.Durable);
        public IList<ISubscriber> Subscribers { get;  } = new List<ISubscriber>();

        public void StubAllExternallyOutgoingEndpoints()
        {
            Advanced.StubAllOutgoingExternalSenders = true;
        }

        public Endpoint[] AllEndpoints()
        {
            return _transports.Values.SelectMany(x => x.Endpoints()).ToArray();
        }

        async Task IDescribedSystemPart.Write(TextWriter writer)
        {
            foreach (var transport in _transports.Values.Where(x => x.Endpoints().Any()))
            {
                await writer.WriteLineAsync(transport.Name);

                foreach (var endpoint in transport.Endpoints())
                {
                    await writer.WriteLineAsync(
                        $"{endpoint.Uri}, Incoming: {endpoint.IsListener}, Reply Uri: {endpoint.IsUsedForReplies}");
                }

                await writer.WriteLineAsync();
            }
        }

        string IDescribedSystemPart.Title => "Jasper Messaging Endpoints";

        Task IWriteToConsole.WriteToConsole()
        {
            var tree = new Tree("Transports and Endpoints");

            foreach (var transport in _transports.Values.Where(x => x.Endpoints().Any()))
            {
                var transportNode = tree.AddNode($"[bold]{transport.Name}[/] [dim]({transport.Protocols.Join(", ")}[/])");
                if (transport is ITreeDescriber d)
                {
                    d.Describe(transportNode);
                }

                foreach (var endpoint in transport.Endpoints())
                {
                    var endpointTitle = endpoint.Uri.ToString();
                    if (endpoint.IsUsedForReplies || object.ReferenceEquals(endpoint, transport.ReplyEndpoint()))
                    {
                        endpointTitle += $" ([bold]Used for Replies[/])";
                    }

                    var endpointNode = transportNode.AddNode(endpointTitle);

                    if (endpoint.IsListener)
                    {
                        endpointNode.AddNode("[bold green]Listener[/]");
                    }

                    var props = endpoint.DescribeProperties();
                    if (props.Any())
                    {
                        var table = BuildTableForProperties(props);

                        endpointNode.AddNode(table);
                    }

                    if (endpoint.Subscriptions.Any())
                    {
                        var subscriptions = endpointNode.AddNode("Subscriptions");
                        foreach (var subscription in endpoint.Subscriptions)
                        {
                            subscriptions.AddNode($"{subscription} ({subscription.ContentTypes.Join(", ")})");
                        }
                    }

                }
            }

            AnsiConsole.Render(tree);

            return Task.CompletedTask;
        }

        // TODO -- this should be in Oakton
        [Obsolete]
        public static Table BuildTableForProperties(IDictionary<string, object> props)
        {
            var table = new Table();
            table.AddColumn("Property");
            table.AddColumn("Value");

            foreach (var prop in props)
            {
                table.AddRow(prop.Key, prop.Value?.ToString() ?? string.Empty);
            }

            return table;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var transport in _transports.Values)
            {
                if (transport is IAsyncDisposable ad)
                {
                    await ad.DisposeAsync();
                }
                else if (transport is IDisposable d)
                {
                    d.Dispose();
                }
            }
        }
    }
}
