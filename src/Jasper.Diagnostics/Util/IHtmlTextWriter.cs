using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Jasper.Diagnostics.Util
{
    public interface IHtmlTextWriter
    {
        Task WriteAsync(string text);
        Task WriteAsync(string text, Encoding encoding);
    }

    public class ResponseHtmlTextWriter : IHtmlTextWriter
    {
        private Stream _body;

        public ResponseHtmlTextWriter(Stream body)
        {
            _body = body;
        }

        public Task WriteAsync(string text)
        {
            return WriteAsync(text, Encoding.UTF8);
        }

        public Task WriteAsync(string text, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(text);
            return _body.WriteAsync(data, 0, data.Length);
        }
    }
}
