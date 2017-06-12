using System.Threading.Tasks;
using Jasper.Codegen;
using Shouldly;
using Xunit;
using Xunit.Sdk;

namespace Jasper.Testing.Codegen
{
    public class MethodCallTester
    {
        [Fact]
        public void determine_return_value_of_simple_type()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetValue());
            @call.ReturnVariable.ShouldNotBeNull();

            @call.ReturnVariable.VariableType.ShouldBe(typeof(string));
            @call.ReturnVariable.Usage.ShouldBe("result_of_GetValue");
            @call.ReturnVariable.Creator.ShouldBeSameAs(@call);
        }

        [Fact]
        public void determine_return_value_of_not_simple_type()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetError());
            @call.ReturnVariable.ShouldNotBeNull();

            @call.ReturnVariable.VariableType.ShouldBe(typeof(ErrorMessage));
            @call.ReturnVariable.Usage.ShouldBe(Variable.DefaultArgName(typeof(ErrorMessage)));
            @call.ReturnVariable.Creator.ShouldBeSameAs(@call);
        }

        [Fact]
        public void no_return_variable_on_void_method()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.Go(null));
            @call.ReturnVariable.ShouldBeNull();
        }

        [Fact]
        public void determine_return_value_of_Task_of_T_simple()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetString());
            @call.ReturnVariable.ShouldNotBeNull();

            @call.ReturnVariable.VariableType.ShouldBe(typeof(string));
            @call.ReturnVariable.Usage.ShouldBe("result_of_GetString");
            @call.ReturnVariable.Creator.ShouldBeSameAs(@call);
        }


        [Fact]
        public void determine_return_value_of_not_simple_type_in_a_task()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetAsyncError());
            @call.ReturnVariable.ShouldNotBeNull();

            @call.ReturnVariable.VariableType.ShouldBe(typeof(ErrorMessage));
            @call.ReturnVariable.Usage.ShouldBe(Variable.DefaultArgName(typeof(ErrorMessage)));
            @call.ReturnVariable.Creator.ShouldBeSameAs(@call);
        }
    }

    public class MethodCallTarget
    {
        public string GetValue()
        {
            return "foo";
        }

        public ErrorMessage GetError()
        {
            return null;
        }

        public Task<ErrorMessage> GetAsyncError()
        {
            return null;
        }

        public void Go(string text)
        {
            
        }

        public Task<string> GetString()
        {
            return Task.FromResult("foo");
        }
    }
}