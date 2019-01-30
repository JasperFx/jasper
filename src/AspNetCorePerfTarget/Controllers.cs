using Microsoft.AspNetCore.Mvc;

namespace AspNetCorePerfTarget
{
    public interface IHasUrls
    {
        string[] Urls();
        string Method { get; }
    }

    public class DeepEndpoints : Controller, IHasUrls
    {
        [HttpGet("/country")]
        public string get_country()
        {
            return "USA!";
        }

        [HttpGet("/country/state")]
        public string get_country_state()
        {
            return "TX";
        }

        [HttpGet("/country/state/direction")]
        public string get_country_state_direction()
        {
            return "North";
        }

        [HttpGet("/country/state/city")]
        public string get_country_state_city()
        {
            return "Austin";
        }

        [HttpGet("/country/state/zipcode")]
        public string get_country_state_zipcode()
        {
            return "00000";
        }

        [HttpGet("/country/state/county")]
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

    public class DeepController : Controller, IHasUrls
    {
        [HttpGet("/start/{from}/to/{end}")]
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

    public class HomeController : Controller, IHasUrls
    {
        [HttpGet("/")]
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

    public class PostsController : IHasUrls
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

        [HttpPost("/dog/{number}")]
        public void post_dog_number(int number)
        {

        }

        [HttpPost("/dog/{number}/{age}")]
        public void post_dog_name_age(string name, int age)
        {

        }

        [HttpPost("/cat/{name}")]
        public void post_cat_name(string name)
        {

        }



    }



    // DELETE, PUT, POST
    public class SimpleHeadController: Controller, IHasUrls
    {
        [HttpHead("/one")]
        public void head_one(){}

        [HttpHead("/two")]
        public void head_two(){}

        [HttpHead("/three")]
        public void head_three(){}

        [HttpHead("/four")]
        public void head_four(){}

        [HttpHead("/five")]
        public void head_five(){}

        [HttpHead("/six")]
        public void head_six(){}

        [HttpHead("/seven")]
        public void head_seven(){}

        [HttpHead("/eight")]
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

    public class SimpleDeleteController: Controller, IHasUrls
    {
        [HttpDelete("/one")]
        public void head_one(){}

        [HttpDelete("/two")]
        public void head_two(){}

        [HttpDelete("/three")]
        public void head_three(){}

        [HttpDelete("/four")]
        public void head_four(){}

        [HttpDelete("/five")]
        public void head_five(){}

        [HttpDelete("/six")]
        public void head_six(){}

        [HttpDelete("/seven")]
        public void head_seven(){}

        [HttpDelete("/eight")]
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

        public string Method { get; } = "DELETE";

    }

    public class SimplePutController: Controller, IHasUrls
    {
        [HttpPut("/one")]
        public void head_one(){}

        [HttpPut("/two")]
        public void head_two(){}

        [HttpPut("/three")]
        public void head_three(){}

        [HttpPut("/four")]
        public void head_four(){}

        [HttpPut("/five")]
        public void head_five(){}

        [HttpPut("/six")]
        public void head_six(){}

        [HttpPut("/seven")]
        public void head_seven(){}

        [HttpPut("/eight")]
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

        public string Method { get; } = "PUT";

    }

    public class SimplePostController: Controller, IHasUrls
    {
        [HttpPost("/one")]
        public void head_one(){}

        [HttpPost("/two")]
        public void head_two(){}

        [HttpPost("/three")]
        public void head_three(){}

        [HttpPost("/four")]
        public void head_four(){}

        [HttpPost("/five")]
        public void head_five(){}

        [HttpPost("/six")]
        public void head_six(){}

        [HttpPost("/seven")]
        public void head_seven(){}

        [HttpPost("/eight")]
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

        public string Method { get; } = "POST";

    }


}
