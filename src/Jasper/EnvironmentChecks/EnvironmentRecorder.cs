using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Util;

namespace Jasper.EnvironmentChecks
{
    [CacheResolver]
    public class EnvironmentRecorder : IEnvironmentRecorder
    {
        private readonly IList<Exception> _exceptions = new List<Exception>();

        public void Success(string description)
        {
            ConsoleWriter.Write(ConsoleColor.Green, "Success: " + description);
        }

        public void Failure(string description, Exception exception)
        {
            ConsoleWriter.Write(ConsoleColor.Red, "Failure: " + description);
            ConsoleWriter.Write(ConsoleColor.Yellow, exception.ToString());

            _exceptions.Add(exception);
        }

        public void AssertAllSuccessful()
        {
            if (_exceptions.Any())
            {
                throw new AggregateException(_exceptions);
            }
        }
    }
}
