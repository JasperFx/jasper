using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace HttpTests.Kestrel
{
    public class DefaultApp : IDisposable
    {
        public DefaultApp()
        {
            Host = WebHost.CreateDefaultBuilder()
                .UseKestrel(o => o.ListenLocalhost(5025))
                .UseUrls("http://localhost:5025")
                .UseStartup<Startup>()
                .UseJasper(x =>
                {
                    x.Http(opts => opts
                        .DisableConventionalDiscovery()
                        .IncludeType<HomeEndpointGuy>()
                        .IncludeType<UserController>());

                    x.Services.AddSingleton(new UserRepository());
                }).Start();
        }

        public IWebHost Host { get; }

        public void Dispose()
        {
            Host?.Dispose();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }

    public class integration_with_kestrel : IClassFixture<DefaultApp>
    {
        public integration_with_kestrel(DefaultApp app, ITestOutputHelper output)
        {
            _app = app;
            _output = output;
        }

        private readonly DefaultApp _app;
        private readonly ITestOutputHelper _output;

        [Fact]
        public async Task get_404_when_resource_is_null()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:5025/user/Rey");

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task get_home_endpoint_get()
        {
            var client = new HttpClient();
            var text = await client.GetStringAsync("http://localhost:5025");

            text.ShouldBe("Hello, world!");
        }

        [Fact]
        public async Task get_json_resource()
        {
            var client = new HttpClient();
            var json = await client.GetStringAsync("http://localhost:5025/user/Luke");

            var user = JsonConvert.DeserializeObject<User>(json);

            user.Name.ShouldBe("Luke");
        }

        [Fact]
        public async Task post_json()
        {
            var user = new User {Name = "Yoda"};

            var json = JsonConvert.SerializeObject(user);

            var request = new HttpRequestMessage
            {
                Method = new HttpMethod("POST"),
                Content = new StringContent(json, Encoding.Default),
                RequestUri = new Uri("http://localhost:5025/user")
            };

            var client = new HttpClient();
            var response = await client.SendAsync(request);


            response.StatusCode.ShouldBe(HttpStatusCode.Created);
        }
    }


    public class HomeEndpointGuy
    {
        [HttpGet("")]
        public string Get()
        {
            return "Hello, world!";
        }
    }

    public class User
    {
        public string Name { get; set; }

        protected bool Equals(User other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }
    }

    public class UserRepository
    {
        public readonly IList<User> Users = new List<User>();

        public UserRepository()
        {
            Users.Add(new User {Name = "Luke"});
            Users.Add(new User {Name = "Leia"});
        }
    }

    public class UserController
    {
        public static User get_user_name(string name, UserRepository users)
        {
            return users.Users.FirstOrDefault(x => x.Name == name);
        }

        public int post_user(User user, UserRepository users)
        {
            users.Users.Add(user);

            return 201; // Created
        }

        public static User[] get_users(UserRepository users)
        {
            return users.Users.ToArray();
        }
    }
}
