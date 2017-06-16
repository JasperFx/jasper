using System.Collections.Generic;

namespace Jasper.Bus.Configuration
{
    internal interface IContentTypeAware
    {
        IEnumerable<string> Accepts { get; }
    }
}