using System;

namespace BlueMilk.IoC
{
    public class ContainerValidationException : Exception
    {
        public ContainerValidationException(string message) : base(message)
        {
        }
    }
}