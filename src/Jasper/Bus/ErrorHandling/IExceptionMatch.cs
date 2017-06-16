using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.ErrorHandling
{
    public interface IExceptionMatch
    {
        bool Matches(Envelope envelope, Exception ex);
    }
}