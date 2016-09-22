using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Codegen.Compilation;
using Jasper.Configuration;

namespace Jasper.Codegen
{
    public class HandlerChain : Chain<Frame, HandlerChain>, IVariableSource, IVariable
    {
        private readonly GenerationConfig _config;
        private readonly Type _inputType;
        private readonly string _inputArg;
        private readonly Type _handlerInterface;

        public HandlerChain(GenerationConfig config, string className, Type inputType, string inputArg, Type handlerInterface)
        {
            _config = config;
            _inputType = inputType;
            _inputArg = inputArg;
            _handlerInterface = handlerInterface;
            ClassName = className;
        }

        public string ClassName { get; }

        public readonly IList<InjectedField> Fields = new List<InjectedField>();

        bool IVariableSource.Matches(Type type)
        {
            return type == _inputType;
        }

        IVariable IVariableSource.Create(Type type)
        {
            return this;
        }

        string IVariable.Name => _inputArg;

        Type IVariable.VariableType => _inputType;

        public IVariable FindVariable(Type type)
        {
            // TODO -- needs to cache the variables!!!!!
            if (type == _inputType) return this;

            var source = _config.Sources.FirstOrDefault(x => x.Matches(type));
            if (source == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type), $"Jasper doesn't know how to build a variable of type '{type.FullName}'");
            }

            return source.Create(type);
        }

        public void Write(ISourceWriter writer)
        {
            writer.Namespace(_config.ApplicationNamespace);

            writeClassDeclaration(writer);

            // Namespace declaration
            writer.FinishBlock();
        }

        private void writeClassDeclaration(ISourceWriter writer)
        {
            writer.Write($"BLOCK:public class {ClassName} : {_handlerInterface.FullName}");

            writeFields(writer);
            writeConstructor(writer);

            writer.BlankLine();

            writeHandleMethod(writer);

            // Class declaration
            writer.FinishBlock();
        }

        private void writeHandleMethod(ISourceWriter writer)
        {
            writer.Write($"BLOCK:public async Task Handle({_inputType.FullName} {_inputArg})");

            Top.GenerateCode(this, writer);

            writer.FinishBlock();
        }

        private void writeFields(ISourceWriter writer)
        {
            foreach (var field in Fields)
            {
                field.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private void writeConstructor(ISourceWriter writer)
        {
            var ctorArgs = Fields.Select(x => x.CtorArgDeclaration).Join(", ");
            writer.Write($"BLOCK:public class {ClassName}({ctorArgs})");

            foreach (var field in Fields)
            {
                field.WriteAssignment(writer);
            }

            writer.FinishBlock();
        }
    }
}