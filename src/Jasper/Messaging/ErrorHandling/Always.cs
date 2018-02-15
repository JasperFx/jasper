using System;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.ErrorHandling
{

    public class Always : IExceptionMatch
    {
        public static readonly Always Instance = new Always();

        private Always()
        {
        }

        public bool Matches(Envelope envelope, Exception ex)
        {
            return true;
        }

        public override string ToString()
        {
            return "Always";
        }
    }
}