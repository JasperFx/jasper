using System;
using System.Linq;
using Baseline;

namespace Jasper.Runtime
{
    public class InvalidSubscriptionException : Exception
    {
        public InvalidSubscriptionException(string[] errors) : base(
            $"Subscription errors detected:{Environment.NewLine}{errors.Select(e => $"* {e}").Join(Environment.NewLine)}")
        {
        }
    }
}
