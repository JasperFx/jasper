namespace benchmarks.Routes
{
    public class DeepGetEndpoint : IHasUrls
    {
        public string get_start_from_to_end(int from, int end)
        {
            return "something";
        }

        public string[] Urls()
        {
            return new string[]
            {
                "/start/3/to/5",
                "/start/1/to/6",
                "/start/5/to/90",
                "/start/11/to/56",
            };
        }

        public string Method { get; } = "GET";
    }
}
