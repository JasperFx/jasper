using System;
using System.Collections.Generic;
using Baseline;

namespace Jasper.Util
{
    internal static class ConsoleWriter
    {
        public const int DefaultConsoleWidth = 120;

        private static readonly int _consoleWidth = DefaultConsoleWidth;

        public static string HL { get; private set;}

        static ConsoleWriter()
        {
            try
            {
                _consoleWidth = Console.BufferWidth;
            }
            catch
            {
                // Console.BufferWidth(get) will throw exceptions in certain circumstances
            }

            if (_consoleWidth < 10) // Mono will return 0 instead of throwing an exception
            {
                _consoleWidth = DefaultConsoleWidth;
            }

            HL = new string('-', _consoleWidth);
        }

        public static int ConsoleBufferWidth => _consoleWidth;

        public static void Line()
        {
            Console.WriteLine();
        }

        public static void PrintHorizontalLine()
        {
            Console.WriteLine(HL);
        }

        public static void PrintHorizontalLine(int indent)
        {
            Console.WriteLine(new string(' ', indent) + HL.Substring(indent));
        }

        public static void Write(string stuff)
        {
            Write(ConsoleColor.White, stuff);
        }

        public static void WriteWithIndent(ConsoleColor color, int indent, string content)
        {
            Console.ForegroundColor = color;
            BreakIntoLines(indent, content)
                .Each(l => Console.WriteLine((string) l));
            Console.ResetColor();
        }
        public static void Write(ConsoleColor color, string content)
        {
            Console.ForegroundColor = color;
            BreakIntoLines(content)
                .Each(l=>Console.WriteLine(l));
            Console.ResetColor();
        }


        public static void Write(string format, params object[] args)
        {
            var input = string.Format(format, args);
            Write(input);
        }

        private static string[] BreakIntoLines(int indent, string input)
        {
            if (string.IsNullOrEmpty(input)) return new string[0];

            var lines = new List<string>();


            while (input.Length > 0)
            {
                var width = _consoleWidth - indent;
                var chomp = input.Length > width ? width : input.Length;

                string c = new string(' ', indent) + input.Substring(0, chomp);

                lines.Add(c);
                input = input.Remove(0, chomp);
            }

            return lines.ToArray();
        }

        private static string[] BreakIntoLines(string input)
        {
            if (string.IsNullOrEmpty(input)) return new string[0];

            var lines = new List<string>();


            while(input.Length > 0)
            {
                var chomp = input.Length > _consoleWidth ? _consoleWidth : input.Length;
                string c = input.Substring(0, chomp);
                lines.Add(c);
                input = input.Remove(0, chomp);
            }

            return lines.ToArray();
        }

        public static void Write(ConsoleColor color, Action action)
        {
            Console.ForegroundColor = color;
            action();
            Console.ResetColor();
        }
    }
}
