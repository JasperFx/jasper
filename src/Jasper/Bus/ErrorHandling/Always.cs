using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.ErrorHandling
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