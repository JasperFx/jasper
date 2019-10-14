using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Conneg
{
    public class ReaderCollection : IEnumerable<IMessageDeserializer>
    {
        private readonly Dictionary<string, IMessageDeserializer> _readers =
            new Dictionary<string, IMessageDeserializer>();

        public ReaderCollection(IMessageDeserializer[] readers)
        {
            foreach (var reader in readers) _readers[reader.ContentType] = reader;

            HasAnyReaders = _readers.Any();

            // Need to prefer any kind of custom reader
            var allContentTypes = _readers.Keys.ToList();
            var index = allContentTypes.IndexOf("application/json");
            if (index >= 0)
            {
                allContentTypes.Remove("application/json");
                allContentTypes.Add("application/json");
            }

            ContentTypes = allContentTypes.ToArray();
        }

        public string[] ContentTypes { get; }

        public IMessageDeserializer this[string contentType] =>
            _readers.ContainsKey(contentType) ? _readers[contentType] : null;

        public bool HasAnyReaders { get; }

        public IEnumerator<IMessageDeserializer> GetEnumerator()
        {
            return _readers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
