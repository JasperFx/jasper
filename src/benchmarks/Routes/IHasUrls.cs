namespace benchmarks.Routes
{
    public interface IHasUrls
    {
        string[] Urls();
        string Method { get; }
    }
}
