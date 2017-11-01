using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Jasper.Bus.Transports.Util
{
    internal static class StreamExtensions
    {
        internal static async Task<byte[]> ReadBytesAsync(this Stream stream, long? length)
        {
            byte[] buffer = new byte[length];
            int totalRead = 0;
            int current;
            do
            {
                current = await stream.ReadAsync(buffer, totalRead, buffer.Length - totalRead).ConfigureAwait(false);
                totalRead += current;
            }
            while (totalRead < length && current > 0);
            return buffer;
        }

        internal static async Task<bool> ReadExpectedBuffer(this Stream stream, byte[] expected)
        {
            try
            {
                var bytes = await stream.ReadBytesAsync(expected.Length).ConfigureAwait(false);
                return expected.SequenceEqual(bytes);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Task SendBuffer(this Stream stream, byte[] buffer)
        {
            return stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
