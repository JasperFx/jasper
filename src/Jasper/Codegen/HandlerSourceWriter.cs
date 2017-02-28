using System.Linq;
using Baseline;
using Jasper.Codegen.Compilation;
using StructureMap.TypeRules;

namespace Jasper.Codegen.New
{
    public static class HandlerSourceWriter
    {
        public static void Write(IHandlerGeneration generation, ISourceWriter writer)
        {
            writeClassDeclaration(generation, writer);
            writeFields(generation, writer);

            writer.BlankLine();

            writeConstructor(generation, writer);

            writer.BlankLine();

            writeHandleMethod(generation, writer);

            writer.FinishBlock();
        }

        private static void writeClassDeclaration(IHandlerGeneration generation, ISourceWriter writer)
        {
            writer.Write($"BLOCK:public class {generation.ClassName} : {generation.BaseType.FullName}");
        }

        private static void writeHandleMethod(IHandlerGeneration generation, ISourceWriter writer)
        {
            var returnValue = generation.AsyncMode == AsyncMode.AsyncTask
                ? "async Task"
                : "Task";

            if (generation.BaseType.IsInterfaceOrAbstract())
            {
                returnValue = "override " + returnValue;
            }

            var variable = generation.InputVariable;
            writer.Write($"BLOCK:public {returnValue} Handle({variable.VariableType.FullName} {variable.Usage})");

            generation.Top.GenerateCode(generation, writer);

            if (generation.AsyncMode == AsyncMode.ReturnCompletedTask)
            {
                writer.Write("return Task.CompletedTask;");
            }

            writer.FinishBlock();
        }

        private static void writeFields(IHandlerGeneration generation, ISourceWriter writer)
        {
            foreach (var field in generation.Fields)
            {
                field.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private static void writeConstructor(IHandlerGeneration generation, ISourceWriter writer)
        {
            var ctorArgs = generation.Fields.Select(x => x.CtorArgDeclaration).Join(", ");
            writer.Write($"BLOCK:public {generation.ClassName}({ctorArgs})");

            foreach (var field in generation.Fields)
            {
                field.WriteAssignment(writer);
            }

            writer.FinishBlock();
        }
    }
}