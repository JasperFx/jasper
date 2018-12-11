namespace Jasper.Messaging.Runtime.Cascading
{
    public static class ContinueImmediately
    {
        public static ImmediateContinuation With(params object[] actions)
        {
            return new ImmediateContinuation(actions);
        }
    }
}
