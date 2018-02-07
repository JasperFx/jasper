using System.IO;

namespace BlueMilk.Util.TextWriting
{
    public interface Line
    {
        void WriteToConsole();
        void Write(TextWriter writer);
        int Width { get; }
    }
}