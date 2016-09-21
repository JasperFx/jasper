namespace Jasper.Configuration
{
    public interface INode<T>
    {
        void AddAfter(T node);
        void AddBefore(T node);

    }
}