using System;
using System.IO;
using System.Threading.Tasks;

namespace Jasper.Conneg
{
    public interface IMediaReader
    {
        string MessageType { get; }
        Type DotNetType { get; }

        string ContentType { get; }
        object Read(byte[] data);
        Task<T> Read<T>(Stream stream);
    }
}
