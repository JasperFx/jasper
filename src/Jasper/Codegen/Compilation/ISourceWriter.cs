namespace Jasper.Codegen.Compilation
{
    public interface ISourceWriter
    {
        void BlankLine();
        void Write(string text = null);
        void FinishBlock(string extra = null);
    }
}