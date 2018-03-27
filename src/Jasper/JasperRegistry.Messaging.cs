using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;

namespace Jasper
{
    public partial class JasperRegistry
    {
        internal MessagingConfiguration Messaging { get; } = new MessagingConfiguration();
        protected internal MessagingSettings MessagingSettings => Messaging.Settings;

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
        public ITransportsExpression Transports => Messaging.Settings;

        /// <summary>
        ///     Configure dynamic subscriptions to this application
        /// </summary>
        public ISubscriptions Subscribe => Messaging.Capabilities;

        /// <summary>
        ///     Gets or sets the logical service name for this Jasper application. By default,
        ///     this is derived from the name of the JasperRegistry class
        /// </summary>
        public string ServiceName
        {
            get => Messaging.Settings.ServiceName;
            set => Messaging.Settings.ServiceName = value;
        }

        /// <summary>
        ///     Configure uncommonly used, advanced options
        /// </summary>
        public IAdvancedOptions Advanced => Messaging.Settings;



    }
}
