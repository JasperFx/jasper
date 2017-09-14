using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Conneg
{
    public class ModelWriter : IEnumerable<IMessageSerializer>
    {
        private readonly Dictionary<string, IMessageSerializer> _writers
            = new Dictionary<string, IMessageSerializer>();

        private readonly ConcurrentDictionary<string, IMessageSerializer> _selections
            = new ConcurrentDictionary<string, IMessageSerializer>();

        private readonly string _defaultMimeType;

        public ModelWriter(IMessageSerializer[] writers)
        {
            _defaultMimeType = writers.FirstOrDefault()?.ContentType;

            foreach (var writer in writers)
            {
                _writers[writer.ContentType] = writer;
            }

            ContentTypes = _writers.Keys.ToArray();
        }

        public string[] ContentTypes { get; }

        public IMessageSerializer this[string contentType] => _writers[contentType];


        public bool TryWrite(string accepted, object model, out string contentType, out byte[] data)
        {
            if (_writers.Count == 0)
            {
                contentType = null;
                data = null;
                return false;
            }

            var writer = ChooseWriter(accepted);
            if (writer == null)
            {
                contentType = null;
                data = null;
                return false;
            }
            else
            {
                contentType = writer.ContentType;
                data = writer.Write(model);

                return true;
            }
        }

        public IMessageSerializer ChooseWriter(string accepted)
        {
            return _selections.GetOrAdd(accepted ?? _defaultMimeType, @select);
        }

        private IMessageSerializer select(string contentType)
        {
            if (!_writers.Any()) return null;

            if (_writers.ContainsKey(contentType))
            {
                return _writers[contentType];
            }

            var mimeTypes = new MimeTypeList(contentType);
            foreach (var mimeType in mimeTypes)
            {
                if (_writers.ContainsKey(mimeType))
                {
                    return _writers[mimeType];
                }
            }

            if (mimeTypes.AcceptsAny() && _writers.Any() && _writers.ContainsKey(_defaultMimeType))
            {
                return _writers[_defaultMimeType];
            }

            return null;
        }

        public IEnumerator<IMessageSerializer> GetEnumerator()
        {
            return _writers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
