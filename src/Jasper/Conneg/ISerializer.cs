using System.IO;
using Baseline;

namespace Jasper.Conneg
{
    public interface ISerializer
    {
        void Serialize(object message, Stream stream);
        object Deserialize(Stream message);

        string ContentType { get; }
    }
}
