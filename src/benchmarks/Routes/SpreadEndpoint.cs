using Baseline;

namespace benchmarks.Routes
{
    public class SpreadEndpoint : IHasUrls
    {
        public static string get_folder(string relativePath)
        {
            return relativePath;
        }

        public static string get_file(string[] pathSegments)
        {
            return pathSegments.Join("-");
        }

        public string[] Urls()
        {
            return new string[]
            {
                "/folder/one/two",
                "/folder/one/two/three",
                "/folder/one/two/four",
                "/folder/one/two/five",
                "/file/one/two/five",
                "/file/one/two/three",
                "/file/one/two/two",

            };
        }

        public string Method { get; } = "GET";
    }
}
