using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jasper.Conneg
{
    public interface IMediaWriter
    {
        Type DotNetType { get; }

        string ContentType { get; }
        byte[] Write(object model);
        Task WriteToStream(object model, HttpResponse response);
    }
}
