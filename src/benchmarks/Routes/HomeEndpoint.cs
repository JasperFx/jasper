namespace benchmarks.Routes
{
    public class HomeEndpoint : IHasUrls
    {
        public string Get()
        {
            return "home";
        }

        public string[] Urls()
        {
            return new string[] {"/"};
        }

        public string Method { get; } = "GET";
    }


}
