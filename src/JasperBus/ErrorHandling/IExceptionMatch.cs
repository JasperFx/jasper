using System;
using JasperBus.Runtime;

namespace JasperBus.ErrorHandling
{
    public interface IExceptionMatch
    {
        bool Matches(Envelope envelope, Exception ex);
    }
}