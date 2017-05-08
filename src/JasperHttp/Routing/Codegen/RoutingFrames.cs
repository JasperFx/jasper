using System;
using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;

namespace JasperHttp.Routing.Codegen
{
    public static class RoutingFrames
    {
        public static readonly string Segments = "segments";

        public static Dictionary<Type, string> TypeOutputs = new Dictionary<Type, string>
        {
            {typeof(bool), "bool"},
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(char), "char"},
            {typeof(decimal), "decimal"},
            {typeof(float), "float"},
            {typeof(short), "short"},
            {typeof(int), "int"},
            {typeof(long), "long"},
            {typeof(ushort), "ushort"},
            {typeof(uint), "uint"},
            {typeof(ulong), "ulong"},
            {typeof(Guid), "Guid"},
        };

        public static bool CanBeRouteArgument(Type type)
        {
            if (type == null) return false;
            return type == typeof(string) || TypeOutputs.ContainsKey(type);
        }

        public static bool CanParse(Type argType)
        {
            return TypeOutputs.ContainsKey(argType);
        }
    }


    public class ParsedRouteArgument : Frame
    {
        public int Position { get; }



        public ParsedRouteArgument(Type type, string name, int position) : base(true)
        {
            Position = position;
            Variable = new Variable(type, name);
        }

        public Variable Variable { get; }

        public override IEnumerable<Variable> Creates
        {
            get { yield return Variable; }
        }

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            Segments = chain.FindVariableByName(typeof(string[]), RoutingFrames.Segments);
            yield return Segments;
        }

        public Variable Segments { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            throw new NotImplementedException();
        }


    }

    public class StringRouteArgument : Frame
    {
        public string Name { get; }
        public int Position { get; }

        public StringRouteArgument(string name, int position) : base(false)
        {
            Name = name;
            Position = position;

            Variable = new Variable(typeof(string), Name, this);
        }

        public override IEnumerable<Variable> Creates { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var {Name} = {RoutingFrames.Segments}[{Position}]");
            writer.BlankLine();

            Next?.GenerateCode(method, writer);
        }

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            Segments = chain.FindVariableByName(typeof(string[]), RoutingFrames.Segments);
            yield return Segments;
        }

        public Variable Variable { get; }
        public Variable Segments { get; private set; }
    }
}
