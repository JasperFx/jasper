using System.Collections.Generic;
using System.Linq;
using Baseline;
using Baseline.Reflection;
using Jasper.Http.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.Routing
{
    public class Reverse_Url_Lookup_Tests
    {
        static Reverse_Url_Lookup_Tests()
        {
            readType<OneController>();
            readType<TwoController>();
            readType<OnlyOneActionController>();
        }

        private static readonly UrlGraph graph = new UrlGraph();
        private static IUrlRegistry urls => graph;

        private static void readType<T>()
        {
            typeof(T).GetMethods().Where(x => x.DeclaringType != typeof(object)).Each(method =>
            {
                var route = JasperRoute.Build(typeof(T), method);
                graph.Register(route);
            });
        }
        // ENDSAMPLE


        // SAMPLE: LookupByInputType
        public static void LookupByInputType(IUrlRegistry urls)
        {
            // Find the url that would handle the CreateUser
            // type as a request body that responds to "PUT"
            var url = urls.UrlForType<CreateUser>("PUT");

            // Look up the Url if you already have the request
            // body
            var input = new CreateUser();
            var url2 = urls.UrlFor(input);
        }
        // ENDSAMPLE

        // SAMPLE: LookupByMethod
        public static void LookupByMethod(IUrlRegistry urls)
        {
            // By type and method name
            var url = urls
                .UrlFor(typeof(UserEndpoints), nameof(UserEndpoints.post_user));


            // If you already have the MethodInfo somehow:/
            // ReflectionHelper is from the Baseline library

            var method = ReflectionHelper.GetMethod<UserEndpoints>(x => x.post_user(null));

            var url2 = urls
                .UrlFor(typeof(UserEndpoints), method);


            // Or by expression
            var url3 = urls.UrlFor<UserEndpoints>(x => x.post_user(null));
        }


        [Fact]
        public void find_by_handler_type_if_only_one_method()
        {
            urls.UrlForType<OnlyOneActionController>()
                .ShouldBe("/go");
        }

        [Fact]
        public void find_route_by_name_positive()
        {
            urls.UrlFor("A").ShouldBe("/one/a");
        }

        [Fact]
        public void retrieve_a_url_by_action()
        {
            urls.UrlFor<OneController>(x => x.delete_one_m2()).ShouldBe("/one/m2");
        }

        [Fact]
        public void retrieve_a_url_by_action_negative_case()
        {
            Exception<UrlResolutionException>.ShouldBeThrownBy(() => { urls.UrlFor<RandomClass>(x => x.Ignored()); });
        }

        [Fact]
        public void retrieve_a_url_for_a_inferred_model_simple_case()
        {
            urls.UrlForType<Model1>().ShouldBe("/one/m1");
        }


        [Fact]
        public void retrieve_a_url_for_a_model_and_http_method()
        {
            urls.UrlFor(new UrlModel(), "GET").ShouldBe("/urlmodel");
            urls.UrlFor(new UrlModel(), "POST").ShouldBe("/urlmodel");
        }

        [Fact]
        public void retrieve_a_url_for_a_model_simple_case()
        {
            urls.UrlFor(new Model1()).ShouldBe("/one/m1");
        }

        [Fact]
        public void retrieve_a_url_for_a_model_that_does_not_exist()
        {
            Exception<UrlResolutionException>.ShouldBeThrownBy(() => { urls.UrlFor(new ModelWithNoChain()); });
        }


        [Fact]
        public void retrieve_by_controller_action_even_if_it_has_an_input_model()
        {
            urls.UrlFor<OneController>(x => x.get_one_M1(null)).ShouldBe("/one/m1");
        }

        [Fact]
        public void retrieve_by_model_with_multiples()
        {
            Exception<UrlResolutionException>.ShouldBeThrownBy(() => { urls.UrlFor(new UrlModel()); });
        }

        // SAMPLE: doing-url-lookup-with-route-arguments
        [Fact]
        public void url_for_arguments()
        {
            // The route pattern for this action is "GET: /range/:from/:to"
            urls.UrlFor<OneController>(x => x.get_range_from_to(1, 5))
                .ShouldBe("/range/1/5");
        }

        [Fact]
        public void url_for_by_type_respects_the_absolute_path()
        {
            urls.UrlForType<Model6>()
                .ShouldBe("/one/a");
        }

        [Fact]
        public void url_for_handler_type_and_method_negative_case_should_throw_204()
        {
            Exception<UrlResolutionException>.ShouldBeThrownBy(() =>
            {
                var method = ReflectionHelper.GetMethod<RandomClass>(x => x.Ignored());
                urls.UrlFor(typeof(OneController), method);
            });
        }

        [Fact]
        public void url_for_handler_type_and_method_positive()
        {
            var method = ReflectionHelper.GetMethod<OneController>(x => x.head_one_m3());

            urls.UrlFor(typeof(OneController), method).ShouldBe("/one/m3");
        }

        [Fact]
        public void url_for_handler_type_and_method_positive_by_method_name()
        {
            urls.UrlFor(typeof(OneController), nameof(OneController.head_one_m3)).ShouldBe("/one/m3");
        }
        // ENDSAMPLE

        // SAMPLE: url_for_named_route_with_arguments
        [Fact]
        public void url_for_named_route_with_arguments()
        {
            var url = urls.UrlFor("GetRange", new Dictionary<string, object> {{"from", 2}, {"to", "6"}});

            url.ShouldBe("/range/2/6");
        }

        // ENDSAMPLE
    }

    public class CreateUser
    {
    }

    public class UserEndpoints
    {
        public int post_user(CreateUser user)
        {
            return 200;
        }
    }


    public class RandomClass
    {
        public void Ignored()
        {
        }
    }

    public class OneController
    {
        [RouteName("find_by_name")]
        public void get_find_Name(ModelWithInputs input)
        {
        }

        [RouteName("A")]
        public void get_one_a(Model6 input)
        {
        }

        public void get_B(Model7 input)
        {
        }


        public void get_one_M1(Model1 input)
        {
        }

        public void delete_one_m2()
        {
        }

        public void head_one_m3()
        {
        }

        public void get_M5(Model3 input)
        {
        }


        public string get_default(DefaultModel model)
        {
            return "welcome to the default view";
        }

        // SAMPLE: get_range_from_to
        // This action would respond to the route GET: /range/:from/:to
        [RouteName("GetRange")]
        public string get_range_from_to(int from, int to)
        {
            return $"From {from} to {to}";
        }

        // ENDSAMPLE
    }

    public class TwoController
    {
        public void get_m1()
        {
        }

        public void get_m2()
        {
        }

        public void get_m3()
        {
        }

        public void get_urlmodel(UrlModel input)
        {
        }

        public void post_urlmodel(UrlModel input)
        {
        }
    }

    public class OnlyOneActionController
    {
        public void get_go(Model8 input)
        {
        }
    }

    public class ModelWithInputs
    {
        public string Name { get; set; }
    }

    public class Model1
    {
    }

    public class Model2
    {
    }

    public class Model3
    {
    }

    public class Model4
    {
    }

    public class Model5
    {
    }

    public class Model6
    {
    }

    public class Model7
    {
    }

    public class Model8
    {
    }

    public class DefaultModel
    {
    }

    public class ModelWithNoChain
    {
    }

    public class ModelWithoutNewUrl
    {
    }


    public class UrlModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class SubclassUrlModel : UrlModel
    {
    }
}
