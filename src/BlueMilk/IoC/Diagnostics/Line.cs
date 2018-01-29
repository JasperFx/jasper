using System.IO;

namespace BlueMilk.IoC.Diagnostics
{
    internal interface Line
    {
        void OverwriteCounts(CharacterWidth[] widths);
        void Write(TextWriter writer, CharacterWidth[] widths);
    }
}