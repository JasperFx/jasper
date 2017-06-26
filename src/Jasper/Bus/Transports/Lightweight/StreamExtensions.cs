using System.IO;
using System.Threading.Tasks;

namespace Jasper.Bus.Transports.Lightweight
{
    public static class StreamExtensions
    {
        public static Task SendBuffer(this Stream stream, byte[] buffer)
        {
            return stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}