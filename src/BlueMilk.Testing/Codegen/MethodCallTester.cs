using System.Threading.Tasks;
using BlueMilk.Codegen;
using Shouldly;
using Xunit;
using Xunit.Sdk;

namespace Jasper.Testing.Internals.Codegen
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

        [Fact]
        public void explicitly_set_parameter_by_variable_type()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.DoSomething(0, 0, null));

            var stringVariable = Variable.For<string>();
            var generalInt = Variable.For<int>();

            // Only one of that type, so it works
            @call.TrySetParameter(stringVariable)
                .ShouldBeTrue();

            @call.Variables[2].ShouldBeSameAs(stringVariable);

            // Multiple parameters of the same type, nothing
            @call.TrySetParameter(generalInt).ShouldBeFalse();
            @call.Variables[0].ShouldBeNull();
            @call.Variables[1].ShouldBeNull();
        }

        [Fact]
        public void explicitly_set_parameter_by_variable_type_and_name()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.DoSomething(0, 0, null));

            var generalInt = Variable.For<int>();

            @call.TrySetParameter("count", generalInt)
                .ShouldBeTrue();

            @call.Variables[0].ShouldBeNull();
            @call.Variables[1].ShouldBeSameAs(generalInt);
            @call.Variables[2].ShouldBeNull();
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

        public void DoSomething(int age, int count, string name)
        {

        }

        public Task<string> GetString()
        {
            return Task.FromResult("foo");
        }
    }
}
