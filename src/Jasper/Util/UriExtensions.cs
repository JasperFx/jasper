using System;
using System.Linq;
using Jasper.Transports;

namespace Jasper.Util;

public static class UriExtensions
{
    public static bool IsDurable(this Uri uri)
    {
        if (uri.Scheme == TransportConstants.Local && uri.Host == TransportConstants.Durable)
        {
            return true;
        }

        var firstSegment = uri.Segments.Skip(1).FirstOrDefault();
        if (firstSegment == null)
        {
            return false;
        }

        return TransportConstants.Durable == firstSegment.TrimEnd('/');
    }
}
