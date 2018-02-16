using System;
using Jasper.Util;

namespace Jasper.Messaging.Logging
{
    public class ConsoleExceptionSink : IExceptionSink
    {
        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            ConsoleWriter.Write(ConsoleColor.Red, message);

            if (correlationId.IsNotEmpty())
            {
                ConsoleWriter.Write(ConsoleColor.Red, $"Id: {correlationId}");
            }

            ConsoleWriter.Write(ConsoleColor.Yellow, ex.ToString());
            Console.WriteLine();
        }
    }
}
