using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Conneg
{
    public class ModelWriter
    {
        private readonly Dictionary<string, IMediaWriter> _writers
            = new Dictionary<string, IMediaWriter>();

        private readonly ConcurrentDictionary<string, IMediaWriter> _selections
            = new ConcurrentDictionary<string, IMediaWriter>();

        private readonly string _defaultMimeType;

        public ModelWriter(IMediaWriter[] writers)
        {
            _defaultMimeType = writers.FirstOrDefault()?.ContentType;

            foreach (var writer in writers)
            {
                _writers[writer.ContentType] = writer;
            }

            ContentTypes = _writers.Keys.ToArray();
        }

        public string[] ContentTypes { get; }


        public bool TryWrite(string accepted, object model, out string contentType, out byte[] data)
        {
            if (_writers.Count == 0)
            {
                contentType = null;
                data = null;
                return false;
            }

            var writer = _selections.GetOrAdd(accepted ?? _defaultMimeType, select);
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

        private IMediaWriter select(string contentType)
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
    }
}
