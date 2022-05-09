using System;

namespace Jasper.Pulsar
{
    public class InvalidPulsarUriException : Exception
    {
        public InvalidPulsarUriException(Uri actualUri) : base($"Invalid Jasper Pulsar Uri '{actualUri}'. Should be of form 'pulsar://persistent/non-persistent/tenant/namespace/topic'")
        {
        }
    }
}
