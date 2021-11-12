using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Serialization
{
    public class ReaderCollection<T> : IEnumerable<T> where T : class, IReaderStrategy
    {
        private readonly Dictionary<string, T> _readers =
            new Dictionary<string, T>();

        public ReaderCollection(T[] readers)
        {
            foreach (var reader in readers) _readers[reader.ContentType] = reader;

            HasAnyReaders = _readers.Any();

            // Need to prefer any kind of custom reader
            var allContentTypes = _readers.Keys.ToList();
            var index = allContentTypes.IndexOf(EnvelopeConstants.JsonContentType);
            if (index >= 0)
            {
                allContentTypes.Remove(EnvelopeConstants.JsonContentType);
                allContentTypes.Add(EnvelopeConstants.JsonContentType);
            }

            ContentTypes = allContentTypes.ToArray();
        }

        public Type MessageType => typeof(T);

        public string[] ContentTypes { get; }

        public T this[string contentType] =>
            _readers.ContainsKey(contentType) ? _readers[contentType] : null;

        public bool HasAnyReaders { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return _readers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
