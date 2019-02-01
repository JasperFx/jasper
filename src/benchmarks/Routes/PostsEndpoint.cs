namespace benchmarks.Routes
{
    public class PostsEndpoint : IHasUrls
    {
        public string[] Urls()
        {
            return new string[]
            {
                "/dog/10",
                "/dog/11",
                "/dog/12",
                "/dog/13",
                "/dog/shiner/15",
                "/dog/spanky/5",
                "/cat/spooky"
            };
        }

        public string Method { get; } = "POST";

        public static void post_dog_number(int number)
        {

        }

        public static void post_dog_name_age(string name, int age)
        {

        }

        public static void post_cat_name(string name)
        {

        }



    }
}
