using System;
using System.IO;
using System.Threading.Tasks;

namespace Jasper.Conneg
{
    public interface IMediaWriter
    {
        Type DotNetType { get; }

        string ContentType { get; }
        byte[] Write(object model);
        Task Write(object model, Stream stream);
    }
}
