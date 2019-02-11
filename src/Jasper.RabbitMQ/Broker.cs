using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public class Broker
    {
        public Broker(Uri uri)
        {
            if (uri.Scheme != "rabbitmq")
                throw new ArgumentOutOfRangeException(nameof(uri), "The protocol must be 'rabbitmq'");
            Uri = uri;


        }


        public Uri Uri { get; }

    }
}
