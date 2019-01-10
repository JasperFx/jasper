using System;
using System.Threading.Tasks;
using Alba;

namespace benchmarks
{
    public class TestRoute
    {
        public string Method { get; }
        public string Url { get; }

        public TestRoute(string method, string url)
        {
            Method = method;
            Url = url;

            if (!Url.StartsWith("/"))
            {
                Url = "/" + url;
            }
        }

        public Task Run(SystemUnderTest system)
        {
            switch (Method)
            {
                case "GET":
                    return system.Scenario(x => x.Get.Url(Url));

                case "POST":
                    return system.Scenario(x => x.Post.Url(Url));

                case "PUT":
                    return system.Scenario(x => x.Put.Url(Url));

                case "DELETE":
                    return system.Scenario(x => x.Delete.Url(Url));

                case "HEAD":
                    return system.Scenario(x => x.Head.Url(Url));
            }

            throw new Exception("Bad request!, the method is " + Method);
        }
    }
}