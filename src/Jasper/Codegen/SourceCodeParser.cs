using System.IO;
using System.Linq;
using Baseline;

namespace Jasper.Codegen
{
    public class SourceCodeParser
    {
        public readonly LightweightCache<string, string> Code = new LightweightCache<string, string>(name => "UNKNOWN");

        private readonly StringWriter _current;
        private readonly string _name;

        public SourceCodeParser(string code)
        {
            foreach (var line in code.ReadLines())
            {
                if (_current == null)
                {
                    if (line.IsEmpty()) continue;

                    if (line.Trim().StartsWith("// START"))
                    {
                        _name = line.Split(':').Last().Trim();
                        _current = new StringWriter();
                    }
                }
                else
                {
                    if (line.Trim().StartsWith("// END"))
                    {
                        var classCode = _current.ToString();
                        Code[_name] = classCode;

                        _current = null;
                        _name = null;
                    }
                    else
                    {
                        _current.WriteLine(line);
                    }
                }

            }
        }
    }
}