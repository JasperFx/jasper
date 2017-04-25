using System;
using System.Linq;
using Baseline;
using Jasper.Codegen.Compilation;
using StructureMap.TypeRules;

namespace Jasper.Codegen.New
{
    [Obsolete("Will be replaced by the new GeneratedClass/GeneratedMethod classes")]
    public static class HandlerSourceWriter
    {
        public static void Write(IGenerationModel generationModel, ISourceWriter writer)
        {
            writeClassDeclaration(generationModel, writer);
            writeFields(generationModel, writer);

            writer.BlankLine();

            writeConstructor(generationModel, writer);

            writer.BlankLine();

            writeHandleMethod(generationModel, writer);

            writer.FinishBlock();
        }

        private static void writeClassDeclaration(IGenerationModel generationModel, ISourceWriter writer)
        {
            writer.Write($"BLOCK:public class {generationModel.ClassName} : {generationModel.BaseType.FullName}");
        }

        private static void writeHandleMethod(IGenerationModel generationModel, ISourceWriter writer)
        {
            var returnValue = generationModel.AsyncMode == AsyncMode.AsyncTask
                ? "async Task"
                : "Task";

            if (generationModel.BaseType.IsInterfaceOrAbstract())
            {
                returnValue = "override " + returnValue;
            }

            var variable = generationModel.InputVariable;
            writer.Write($"BLOCK:public {returnValue} Handle({variable.VariableType.FullName} {variable.Usage})");

            generationModel.Top.GenerateCode(generationModel, writer);

            if (generationModel.AsyncMode == AsyncMode.ReturnCompletedTask)
            {
                writer.Write("return Task.CompletedTask;");
            }

            writer.FinishBlock();
        }

        private static void writeFields(IGenerationModel generationModel, ISourceWriter writer)
        {
            foreach (var field in generationModel.Fields)
            {
                field.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private static void writeConstructor(IGenerationModel generationModel, ISourceWriter writer)
        {
            var ctorArgs = generationModel.Fields.Select(x => x.CtorArgDeclaration).Join(", ");
            writer.Write($"BLOCK:public {generationModel.ClassName}({ctorArgs})");

            foreach (var field in generationModel.Fields)
            {
                field.WriteAssignment(writer);
            }

            writer.FinishBlock();
        }
    }
}
