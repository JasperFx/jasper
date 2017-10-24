﻿using Baseline.Dates;
using Jasper.Bus.Transports.Core;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Lightweight.Protocol
{
    public class queue_does_not_exist_on_receiver : ProtocolContext
    {
        public queue_does_not_exist_on_receiver()
        {
            theReceiver.StatusToReturn = ReceivedStatus.QueueDoesNotExist;

            afterSending().Wait(2.Seconds());
        }

        [Fact]
        public void did_not_succeed()
        {
            theSender.Succeeded.ShouldBeFalse();
        }

        [Fact]
        public void should_tell_the_sender_callback()
        {
            theSender.QueueDoesNotExist.ShouldBeTrue();
        }
    }
}