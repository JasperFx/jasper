using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Conneg
{
    public class WriterCollection<T> : IEnumerable<T> where T : class, IWriterStrategy
    {
        private readonly string _defaultMimeType;

        private readonly ConcurrentDictionary<string, T> _selections
            = new ConcurrentDictionary<string, T>();

        private readonly Dictionary<string, T> _writers
            = new Dictionary<string, T>();

        public WriterCollection(T[] writers)
        {
            _defaultMimeType = writers.FirstOrDefault()?.ContentType;

            foreach (var writer in writers) _writers[writer.ContentType] = writer;

            ContentTypes = _writers.Keys.ToArray();
        }

        public string[] ContentTypes { get; }

        public T this[string contentType] => _writers[contentType];

        public IEnumerator<T> GetEnumerator()
        {
            return _writers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        public T ChooseWriter(string accepted)
        {
            return _selections.GetOrAdd(accepted ?? _defaultMimeType, select);
        }

        private T select(string contentType)
        {
            if (!_writers.Any()) return null;

            if (_writers.ContainsKey(contentType)) return _writers[contentType];

            var mimeTypes = new MimeTypeList(contentType);
            foreach (var mimeType in mimeTypes)
                if (_writers.ContainsKey(mimeType))
                    return _writers[mimeType];

            if (mimeTypes.AcceptsAny() && _writers.Any() && _writers.ContainsKey(_defaultMimeType))
                return _writers[_defaultMimeType];

            return null;
        }
    }
}
