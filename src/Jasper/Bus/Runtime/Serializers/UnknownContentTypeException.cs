using System;
using Baseline;

namespace Jasper.Bus.Runtime.Serializers
{
    public class UnknownContentTypeException : Exception
    {
        public UnknownContentTypeException(string[] contentType, string[] supported) : base($"Unknown content-type(s) {contentType.Join(", ")}, available: {supported.Join(", ")}")
        {
        }

        public UnknownContentTypeException(string contentType, string[] supported) : this(new string[] {contentType},
            supported)
        {

        }


    }
}