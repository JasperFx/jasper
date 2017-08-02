using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jasper.Conneg
{
    public interface IMediaReader
    {
        string MessageType { get; }
        Type DotNetType { get; }

        string ContentType { get; }
        object ReadFromData(byte[] data);
        Task<T> ReadFromRequest<T>(HttpRequest request);
    }
}
