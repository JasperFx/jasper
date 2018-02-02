using System.IO;

namespace Jasper.Util.TextWriting
{
    public interface Line
    {
        void WriteToConsole();
        void Write(TextWriter writer);
        int Width { get; }
    }
}