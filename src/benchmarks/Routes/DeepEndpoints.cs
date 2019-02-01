namespace benchmarks.Routes
{
    public class DeepEndpoints : IHasUrls
    {
        public static string get_country()
        {
            return "USA!";
        }

        public static string get_country_state()
        {
            return "TX";
        }

        public static string get_country_state_direction()
        {
            return "North";
        }

        public static string get_country_state_city()
        {
            return "Austin";
        }

        public static string get_country_state_zipcode()
        {
            return "00000";
        }

        public static string get_country_state_county()
        {
            return "Travis";
        }

        public string[] Urls()
        {
            return new string[]
            {
                "/country",
                "/country/state",
                "/country/state/direction",
                "/country/state/city",
                "/country/state/zipcode",
                "/country/state/county",
            };
        }

        public string Method { get; } = "GET";
    }
}
