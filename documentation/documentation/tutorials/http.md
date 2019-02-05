<!--title:Building HTTP Services with Jasper-->

<[info]>
This tutorial and the *jasper.http* template is geared around HTTP services where you're primarily using Jasper for
the HTTP APIs. If you are using a hybrid MVC Core + Jasper application, see <[linkto:documentation/tutorials/mvc]>
<[/info]>

For the purpose of this tutorial, we're going to heavily leverage the `dotnet` command line tools, so if you would, open your favorite command line tool. 

The first step is just to ensure you have the latest Jasper project templates by installing `JasperTemplates` as shown below:

```
dotnet new -i JasperTemplates
```

Cool, now, to build out the skeleton of a new Jasper HTTP service by going to the command line and using these commands:

```
mkdir MyJasperHttpService
cd MyJasperHttpService
dotnet new jasper.http
```

If you want, go ahead and start up the new HTTP service by typing `dotnet run`, and open your browser to *http://localhost:5000* or *https://localhost:5001* to see the generated "GET: /" endpoint in action as shown below:

<[img:content/HelloFromMyJasperHttpService.png]>

If you open the generated project and look at the constituent parts, you'll see the application entry point in `Program`:

```
    public class Program
    {
        public static int Main(string[] args)
        {
            return CreateWebHostBuilder(args).RunJasper(args);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseJasper<JasperConfig>();
                
    }
```

This template uses the ASP.Net Core `WebHost.CreateDefaultBuilder()` mechanism to lay down the basics of an ASP.Net Core application. On top of that, the template generates a skeleton [Startup class](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-2.2) as you'd use in other ASP.Net Core applications and a `JasperRegistry` class named `JasperConfig` that you can optionally use to configure Jasper-specific options.

See [the ASP.Net documentation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.webhost.createdefaultbuilder?view=aspnetcore-2.2) for more information about what's included in the `WebHost.CreateDefaultBuilder()` configuration.

The ASP.Net `Startup` is a little cut down from the standard *webapi* template and does **not** include MVC Core:

```
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // This is required for ASP.Net Core, but we'd recommend that you keep all of your registrations
            // in one place. So either here, or in the JasperConfig class
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, JasperOptions options)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseHttpsRedirection();
        }
    }
```

Lastly, the `JasperConfig` class is where you can make any Jasper specific configuration:

```
    internal class JasperConfig : JasperRegistry
    {
        public JasperConfig()
        {
            // Add any necessary jasper options
        }
    }
```

See <[linkto:documentation/bootstrapping]> for a lot more information on Jasper's options.

## Get a Resource

Let's say that we're writing some kind of web service that will govern *User* entities. A simple `User` resource might look like this:

```
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
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
```

and to be really crude, let's say that `User` objects are persisted and loaded through an in memory `UserRepository`:

```
    public class UserRepository
    {
        public readonly IList<User> Users = new List<User>();

        public UserRepository()
        {
            Users.Add(new User{Name = "Luke"});
            Users.Add(new User{Name = "Leia"});
        }
    }
```

And we need to add this as a singleton-scoped object in our application's IoC container:

```
    internal class JasperConfig : JasperRegistry
    {
        public JasperConfig()
        {
            Services.AddSingleton<UserRepository>();
        }
    }
```

Now, let's add an HTTP endpoint that will return a `User` document by name as JSON:

```
    public static class UserEndpoints
    {
        public static User get_user_name(string name, UserRepository users)
        {
            return users.Users.FirstOrDefault(x => x.Name == name);
        }
    }
```

I'll start up the application again, and open the browser to "https://localhost:5001/user/Luke":

MORE HERE, HERE, HERE

