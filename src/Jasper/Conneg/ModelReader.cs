using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Jasper.Conneg
{
    public class ModelReader
    {
        private readonly Dictionary<string, IMediaReader> _readers = new Dictionary<string, IMediaReader>();

        public ModelReader(IMediaReader[] readers)
        {
            foreach (var reader in readers)
            {
                _readers[reader.ContentType] = reader;
            }

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

        public IMediaReader this[string contentType] => _readers[contentType];

        public bool TryRead(string contentType, byte[] data, out object model)
        {
            if (!_readers.ContainsKey(contentType))
            {
                model = null;
                return false;
            }

            model = _readers[contentType].Read(data);

            return true;
        }

        public Task<T> TryRead<T>(string contentType, Stream stream) where T : class
        {
            return !_readers.ContainsKey(contentType)
                ? Task.FromResult(default(T))
                : _readers[contentType].Read<T>(stream);
        }

        public bool HasAnyReaders { get; }
    }
}
