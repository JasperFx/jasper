using System.IO;
using System.Threading.Tasks;

namespace Jasper.Conneg
{
    public interface IMediaReader<T>
    {
        string ContentType { get; }
        T Read(byte[] data);
        Task<T> Read(Stream stream);
    }
}