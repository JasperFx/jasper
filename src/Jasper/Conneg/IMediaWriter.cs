using System.IO;
using System.Threading.Tasks;

namespace Jasper.Conneg
{
    public interface IMediaWriter<T>
    {
        string ContentType { get; }
        byte[] Write(T model);
        Task Write(T model, Stream stream);
    }
}