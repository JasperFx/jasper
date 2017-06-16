using System.Collections.Generic;

namespace JasperBus.Configuration
{
    internal interface IContentTypeAware
    {
        IEnumerable<string> Accepts { get; }
    }
}