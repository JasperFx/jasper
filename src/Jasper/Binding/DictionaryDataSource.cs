using System.Collections.Generic;

namespace Jasper.Binding
{
    public class DictionaryDataSource : IDataSource
    {
        public IDictionary<string, string> Dictionary { get; }
        public IDictionary<string, IDataSource> Children { get; } = new Dictionary<string, IDataSource>();

        public DictionaryDataSource(IDictionary<string, string> dictionary)
        {
            Dictionary = dictionary;
        }

        public bool Has(string key)
        {
            return Dictionary.ContainsKey(key);
        }

        public string Get(string key)
        {
            return Dictionary[key];
        }

        public IEnumerable<string> Keys()
        {
            return Dictionary.Keys;
        }

        public bool HasChild(string key)
        {
            return Children.ContainsKey(key);
        }

        public IDataSource GetChild(string key)
        {
            return Children[key];
        }

        public DictionaryDataSource AddChild(string key)
        {
            if (Children.ContainsKey(key))
            {
                return Children[key] as DictionaryDataSource;
            }

            var source = new DictionaryDataSource(new Dictionary<string, string>());
            Children.Add(key, source);

            return source;
        }
    }
}