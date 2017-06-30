using System;
using System.Linq;

namespace Jasper.Util
{
    public static class UriExtensions
    {
        public static string QueueName(this Uri uri)
        {
            return uri.Segments.Last();
        }
    }
}
