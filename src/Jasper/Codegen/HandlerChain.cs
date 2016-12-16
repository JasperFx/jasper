using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Codegen.Compilation;
using Jasper.Configuration;

namespace Jasper.Codegen
{
    // Used to track the compilation within a single chain
    public class HandlerGeneration : Variable
    {
        private readonly GenerationConfig _config;

        public HandlerGeneration(GenerationConfig config, Type inputType, string inputArg)
        : base(inputType, inputArg)
        {
            _config = config;
        }

        // TODO -- may need to track name as well
        public Variable FindVariable(Type type)
        {
            if (type == VariableType) return this;

            var source = _config.Sources.FirstOrDefault(x => x.Matches(type));
            if (source == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type), $"Jasper doesn't know how to build a variable of type '{type.FullName}'");
            }

            return source.Create(type);
        }

        public readonly IList<InjectedField> Fields = new List<InjectedField>();

    }



    public class HandlerChain : Chain<Frame, HandlerChain>
    {
        private readonly Type _handlerInterface;

        public HandlerChain(string className, Type handlerInterface)
        {
            _handlerInterface = handlerInterface;
            ClassName = className;
        }

        public string ClassName { get; }

        public void Write(HandlerGeneration generation, ISourceWriter writer)
        {
            writeClassDeclaration(generation, writer);

            /*
            1.) Call Frame.ResolveVariables on each
            2.) Frame should keep track of which variables are first used in that frame
            3.) Go through each frame, find all variables not previously encountered, and in variable order, have it optionally add frames
            4.) Generate code by calling through the first frame

            */

            // Namespace declaration
            writer.FinishBlock();
        }

        private void writeClassDeclaration(HandlerGeneration generation, ISourceWriter writer)
        {
            writer.Write($"BLOCK:public class {ClassName} : {_handlerInterface.FullName}");

            writeFields(generation, writer);
            writeConstructor(generation, writer);

            writer.BlankLine();

            writeHandleMethod(generation, writer);

            // Class declaration
            writer.FinishBlock();
        }

        private void writeHandleMethod(HandlerGeneration generation, ISourceWriter writer)
        {
            writer.Write($"BLOCK:public async Task Handle({generation.VariableType.FullName} {generation.Name})");

            Top.GenerateCode(generation, writer);

            writer.FinishBlock();
        }

        private void writeFields(HandlerGeneration generation, ISourceWriter writer)
        {
            foreach (var field in generation.Fields)
            {
                field.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private void writeConstructor(HandlerGeneration generation, ISourceWriter writer)
        {
            var ctorArgs = generation.Fields.Select(x => x.CtorArgDeclaration).Join(", ");
            writer.Write($"BLOCK:public class {ClassName}({ctorArgs})");

            foreach (var field in generation.Fields)
            {
                field.WriteAssignment(writer);
            }

            writer.FinishBlock();
        }
    }
}