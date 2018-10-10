using System;
using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;

namespace Jasper
{
    public partial class JasperRegistry : ITransportsExpression
    {
        internal MessagingConfiguration Messaging { get; } = new MessagingConfiguration();

        /// <summary>
        ///     Options to control how Jasper discovers message handler actions, error
        ///     handling, local worker queues, and other policies on message handling
        /// </summary>
        public IHandlerConfiguration Handlers => Messaging.Handling;


        /// <summary>
        ///     Configure static message routing rules and message publishing rules
        /// </summary>
        public PublishingExpression Publish { get; }

        /// <summary>
        ///     Configure or disable the built in transports
        /// </summary>
        public ITransportsExpression Transports => this;

        private string _serviceName;

        /// <summary>
        /// Get or set the logical Jasper service name. By default, this is
        /// derived from the name of a custom JasperRegistry
        /// </summary>
        public string ServiceName
        {
            get => _serviceName;
            set
            {
                _serviceName = value;
                Settings.Messaging(x => x.ServiceName = value);
            }
        }




        void ITransportsExpression.ListenForMessagesFrom(Uri uri)
        {
            Settings.Alter<MessagingSettings>(x => x.ListenForMessagesFrom(uri));
        }

        void ITransportsExpression.ListenForMessagesFrom(string uriString)
        {
            Settings.Alter<MessagingSettings>(x => x.ListenForMessagesFrom(uriString.ToUri()));
        }

        void ITransportsExpression.EnableTransport(string protocol)
        {
            Settings.Alter<MessagingSettings>(x => x.EnableTransport(protocol));
        }

        void ITransportsExpression.DisableTransport(string protocol)
        {
            Settings.Alter<MessagingSettings>(x => x.DisableTransport(protocol));
        }
    }
}
