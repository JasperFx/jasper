namespace benchmarks.Routes
{
    public class SimpleGetEndpoints : IHasUrls
    {
        public static void get_one(){}
        public static void get_two(){}
        public static void get_three(){}
        public static void get_four(){}
        public static void get_five(){}
        public static void get_six(){}
        public static void get_seven(){}
        public static void get_eight(){}

        public string[] Urls()
        {
            return new string[]
            {
                "one",
                "two",
                "three",
                "four",
                "five",
                "six",
                "seven",
                "eight",
            };
        }

        public string Method { get; } = "GET";
    }

    public class SimplePostEndpoints: IHasUrls
    {
        public void post_one(){}
        public void post_two(){}
        public void post_three(){}
        public void post_four(){}
        public void post_five(){}
        public void post_six(){}
        public void post_seven(){}
        public void post_eight(){}

        public string[] Urls()
        {
            return new string[]
            {
                "one",
                "two",
                "three",
                "four",
                "five",
                "six",
                "seven",
                "eight",
            };
        }

        public string Method { get; } = "POST";

    }

    public class SimplePutEndpoints: IHasUrls
    {
        public void put_one(){}
        public void put_two(){}
        public void put_three(){}
        public void put_four(){}
        public void put_five(){}
        public void put_six(){}
        public void put_seven(){}
        public void put_eight(){}

        public string[] Urls()
        {
            return new string[]
            {
                "one",
                "two",
                "three",
                "four",
                "five",
                "six",
                "seven",
                "eight",
            };
        }

        public string Method { get; } = "PUT";

    }

    public class SimpleDeleteEndpoints: IHasUrls
    {
        public void delete_one(){}
        public void delete_two(){}
        public void delete_three(){}
        public void delete_four(){}
        public void delete_five(){}
        public void delete_six(){}
        public void delete_seven(){}
        public void delete_eight(){}

        public string[] Urls()
        {
            return new string[]
            {
                "one",
                "two",
                "three",
                "four",
                "five",
                "six",
                "seven",
                "eight",
            };
        }

        public string Method { get; } = "DELETE";

    }

    public class SimpleHeadEndpoints: IHasUrls
    {
        public void head_one(){}
        public void head_two(){}
        public void head_three(){}
        public void head_four(){}
        public void head_five(){}
        public void head_six(){}
        public void head_seven(){}
        public void head_eight(){}

        public string[] Urls()
        {
            return new string[]
            {
                "one",
                "two",
                "three",
                "four",
                "five",
                "six",
                "seven",
                "eight",
            };
        }

        public string Method { get; } = "HEAD";

    }
}
