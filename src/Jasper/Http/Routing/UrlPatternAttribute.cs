using System;

namespace Jasper.Http.Routing
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class UrlPatternAttribute : Attribute
    {
        public UrlPatternAttribute(string pattern)
        {
            Pattern = pattern.Trim();
        }

        public string Pattern { get; }
    }
}
