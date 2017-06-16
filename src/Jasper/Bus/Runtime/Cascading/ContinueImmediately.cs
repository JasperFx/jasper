namespace Jasper.Bus.Runtime.Cascading
{
    public static class ContinueImmediately
    {
        public static ImmediateContinuation With(params object[] actions)
        {
            return new ImmediateContinuation(actions);
        }
    }
}