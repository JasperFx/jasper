using System;

namespace Jasper.EnvironmentChecks
{
    public static class EnvironmentChecker
    {
        public static void AssertAll(JasperRuntime runtime)
        {
            var recorder = ExecuteAll(runtime);

            recorder.AssertAllSuccessful();
        }

        public static IEnvironmentRecorder ExecuteAll(JasperRuntime runtime)
        {
            var checks = runtime.Container.Model.GetAllPossible<IEnvironmentCheck>();

            var recorder = runtime.Container.GetInstance<IEnvironmentRecorder>();

            foreach (var check in checks)
            {
                try
                {
                    check.Assert(runtime);
                    recorder.Success(check.ToString());
                }
                catch (Exception e)
                {
                    recorder.Failure(check.ToString(), e);
                }
            }
            return recorder;
        }
    }
}
