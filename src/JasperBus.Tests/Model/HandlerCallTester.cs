using System;
using JasperBus.Model;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Model
{
    public class HandlerCallTester
    {
        [Fact]
        public void throws_chunks_if_you_try_to_use_a_method_with_no_inputs()
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(
                () => { HandlerCall.For<ITargetHandler>(x => x.ZeroInZeroOut()); });
        }

        [Fact]
        public void could_handle()
        {
            var handler1 = HandlerCall.For<SomeHandler>(x => x.Interface(null));
            var handler2 = HandlerCall.For<SomeHandler>(x => x.BaseClass(null));

            handler1.CouldHandleOtherMessageType(typeof (Input1)).ShouldBeTrue();
            handler2.CouldHandleOtherMessageType(typeof (Input1)).ShouldBeTrue();

            handler1.CouldHandleOtherMessageType(typeof (Input2)).ShouldBeFalse();
            handler1.CouldHandleOtherMessageType(typeof (Input2)).ShouldBeFalse();
        }

        [Fact]
        public void could_handle_is_false_for_its_own_input_type()
        {
            var handler = HandlerCall.For<ITargetHandler>(x => x.OneInOneOut(null));
            handler.CouldHandleOtherMessageType(typeof (Input)).ShouldBeFalse();
        }

    }
}