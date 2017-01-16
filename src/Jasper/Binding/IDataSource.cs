using System.Collections.Generic;

namespace Jasper.Binding
{
    public interface IDataSource
    {
        bool Has(string key);
        string Get(string key);

        IEnumerable<string> Keys();

        bool HasChild(string key);

        IDataSource GetChild(string key);
    }
}