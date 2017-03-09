using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace JasperBus.Configuration
{
    public class ChannelNode : IContentTypeAware
    {
        public Uri Uri { get; }

        public ChannelNode(Uri uri)
        {
            Uri = uri;
        }

        public readonly List<string> AcceptedContentTypes = new List<string>();

        IEnumerable<string> IContentTypeAware.Accepts => AcceptedContentTypes;
    }

    internal interface IContentTypeAware
    {
        IEnumerable<string> Accepts { get; }
    }
}