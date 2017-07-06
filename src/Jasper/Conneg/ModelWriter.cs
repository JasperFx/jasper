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

        private string _defaultMimeType;

        public ModelWriter(IMediaWriter[] writers)
        {
            _defaultMimeType = writers.FirstOrDefault()?.ContentType;

            foreach (var writer in writers)
            {
                _writers[writer.ContentType] = writer;
            }
        }

        public bool TryWrite(string accepted, object model, out string contentType, out byte[] data)
        {
            var writer = _selections.GetOrAdd(accepted, select);
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

            return mimeTypes.AcceptsAny() ? _selections[_defaultMimeType] : null;
        }
    }
}
