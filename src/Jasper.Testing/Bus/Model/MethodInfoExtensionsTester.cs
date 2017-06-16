using System;
using System.Linq.Expressions;
using System.Reflection;
using Baseline.Reflection;
using Jasper.Bus.Model;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Model
{
    public class MethodInfoExtensionsTester
    {
        private MethodInfo methodFor(Expression<Action<Target>> expression)
        {
            return ReflectionHelper.GetMethod(expression);
        }

        [Fact]
        public void message_type_for_single_parameter_that_is_concrete()
        {
            methodFor(x => x.Go(null)).MessageType()
                .ShouldBe(typeof(Message1));
        }

        [Fact]
        public void message_type_keys_on_name_message()
        {
            methodFor(x => x.Go2(null, null)).MessageType()
                .ShouldBe(typeof(Message1));
        }

        [Fact]
        public void message_type_keys_on_name_input()
        {
            methodFor(x => x.Go3(null, null, null)).MessageType()
                .ShouldBe(typeof(Message2));
        }

        [Fact]
        public void throw_exception_if_you_do_not_follow_that_convention()
        {
            ShouldBeNullExtensions.ShouldBeNull(methodFor(x => x.Go4(null, null)).MessageType());
        }

        [Fact]
        public void throw_exception_if_you_have_no_parameters()
        {
            ShouldBeNullExtensions.ShouldBeNull(methodFor(x => x.Go5()).MessageType());
        }

        public class Target
        {
            public void Go(Message1 message)
            {

            }

            public void Go2(Message1 message, IService service)
            {

            }

            public void Go3(Message2 input, IService service, IService2 service2)
            {

            }

            public void Go4(IService service, IService2 service2)
            {

            }

            public void Go5()
            {

            }
        }

        public interface IService{}
        public interface IService2{}
    }


}