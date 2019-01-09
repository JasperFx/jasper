namespace benchmarks.Routes
{
    public class DeepEndpoints : IHasUrls
    {
        public string get_country()
        {
            return "USA!";
        }

        public string get_country_state()
        {
            return "TX";
        }

        public string get_country_state_direction()
        {
            return "North";
        }

        public string get_country_state_city()
        {
            return "Austin";
        }

        public string get_country_state_zipcode()
        {
            return "00000";
        }

        public string get_country_state_county()
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
