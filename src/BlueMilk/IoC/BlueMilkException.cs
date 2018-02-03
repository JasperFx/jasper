using System;

namespace BlueMilk.IoC
{
    public class BlueMilkException : Exception
    {
        public BlueMilkException(string message) : base(message)
        {
        }

        public BlueMilkException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}