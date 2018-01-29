namespace BlueMilk.IoC
{
    public interface IContainerInitializer
    {
        void Initialize(Scope scope);
    }
}