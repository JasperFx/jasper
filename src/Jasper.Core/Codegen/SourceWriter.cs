using System;
using System.IO;
using Baseline;

namespace Jasper.Core.Codegen
{
    public class SourceWriter
    {
        private readonly StringWriter _writer = new StringWriter();

        private int _level = 0;
        private string _leadingSpaces = "";

        public int IndentionLevel
        {
            get { return _level; }
            set
            {
                _level = value;
                _leadingSpaces = "".PadRight(_level * 4);
            }
        }

        public void WriteLine(string text)
        {
            _writer.WriteLine(_leadingSpaces + text);
        }

        public void BlankLine()
        {
            _writer.WriteLine();
        }

        public void Write(string text = null)
        {
            if (text.IsEmpty())
            {
                BlankLine();
                return;
            }

            text.ReadLines(line =>
            {
                line = line.Replace('`', '"');

                if (line.IsEmpty())
                {
                    BlankLine();
                }
                else if (line.StartsWith("BLOCK:"))
                {
                    WriteLine(line.Substring(6));
                    StartBlock();
                }
                else if (line.StartsWith("END"))
                {
                    FinishBlock(line.Substring(3));
                }
                else
                {
                    WriteLine(line);
                }

            });


        }

        public void StartNamespace(string @namespace)
        {
            WriteLine($"namespace {@namespace}");
            StartBlock();
        }

        private void StartBlock()
        {

            WriteLine("{");
            IndentionLevel++;
        }

        public void FinishBlock(string extra = null)
        {
            IndentionLevel--;

            if (extra.IsEmpty())
            {
                WriteLine("}");
            }
            else
            {
                WriteLine("}" + extra);
            }


            BlankLine();
        }

        public IDisposable InBlock(string declaration = null)
        {
            if (declaration.IsNotEmpty())
            {
                WriteLine(declaration);
            }

            StartBlock();

            return new BlockMarker(this);
        }

        public IDisposable StartClass(string declaration)
        {
            WriteLine(declaration);
            return InBlock();
        }

        public string Code()
        {
            return _writer.ToString();
        }
    }
}