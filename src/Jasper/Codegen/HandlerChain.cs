using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Jasper.Codegen.Compilation;
using Jasper.Configuration;
using Jasper.Internal;

namespace Jasper.Codegen
{
    public class HandlerChain : Chain<Frame, HandlerChain>
    {
        private readonly Type _handlerInterface;

        public HandlerChain(string className, Type handlerInterface)
        {
            if (!handlerInterface.Closes(typeof(IHandler<>)))
            {
                throw new ArgumentOutOfRangeException(nameof(handlerInterface), "Type must close IHandler<T>");
            }

            _handlerInterface = handlerInterface;

            InputType = handlerInterface.FindInterfaceThatCloses(typeof(IHandler<>))
                .GetGenericArguments()[0];
            ClassName = className;
        }

        public Type InputType { get; }

        public string ClassName { get; }

        public void Write(HandlerGeneration generation, ISourceWriter writer)
        {
            foreach (var frame in this)
            {
                // TODO -- may need to tell the generation what the current frame is
                frame.ResolveVariables(generation);
            }

            writeClassDeclaration(generation, writer);
            writeFields(generation, writer);

            writer.BlankLine();

            writeConstructor(generation, writer);

            writer.BlankLine();

            writeHandleMethod(generation, writer);

            writer.FinishBlock();

            /*
            2.) Frame should keep track of which variables are first used in that frame
            3.) Go through each frame, find all variables not previously encountered, and in variable order, have it optionally add frames
            4.) Generate code by calling through the first frame

            */


        }

        private void writeClassDeclaration(HandlerGeneration generation, ISourceWriter writer)
        {
            writer.Write($"BLOCK:public class {ClassName} : {_handlerInterface.FullName}");
        }


        private void writeHandleMethod(HandlerGeneration generation, ISourceWriter writer)
        {
            var returnValue = generation.AsyncMode == AsyncMode.AsyncTask
                ? "async Task"
                : "Task";

            writer.Write($"BLOCK:public {returnValue} Handle({generation.VariableType.FullName} {generation.Name})");

            Top.GenerateCode(generation, writer);

            if (generation.AsyncMode == AsyncMode.ReturnCompletedTask)
            {
                writer.Write("return Task.CompletedTask;");
            }

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
            writer.Write($"BLOCK:public {ClassName}({ctorArgs})");

            foreach (var field in generation.Fields)
            {
                field.WriteAssignment(writer);
            }

            writer.FinishBlock();
        }

        /// <summary>
        /// Adds a MethodCall to the end of this chain
        /// </summary>
        /// <param name="expression"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="NotImplementedException"></exception>
        public void Call<T>(Expression<Action<T>> expression)
        {
            var call = MethodCall.For(expression);

            AddToEnd(call);
        }
    }
}