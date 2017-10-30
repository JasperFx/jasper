using System;

namespace Jasper.EnvironmentChecks
{
    public interface IEnvironmentRecorder
    {
        void Success(string description);
        void Failure(string description, Exception exception);

        void AssertAllSuccessful();
    }
}