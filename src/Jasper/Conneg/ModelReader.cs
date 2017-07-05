using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Jasper.Conneg
{
    public class ModelReader<T> where T : class
    {
        private readonly Dictionary<string, IMediaReader<T>> _readers = new Dictionary<string, IMediaReader<T>>();

        public ModelReader(IMediaReader<T>[] readers)
        {
            foreach (var reader in readers)
            {
                _readers[reader.ContentType] = reader;
            }
        }

        public bool TryRead(string contentType, byte[] data, out T model)
        {
            if (!_readers.ContainsKey(contentType))
            {
                model = null;
                return false;
            }

            model = _readers[contentType].Read(data);

            return true;
        }

        public Task<T> TryRead(string contentType, Stream stream)
        {
            return !_readers.ContainsKey(contentType)
                ? Task.FromResult(default(T))
                : _readers[contentType].Read(stream);
        }
    }
}
