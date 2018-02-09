namespace Jasper.EnvironmentChecks
{
    // SAMPLE: IEnvironmentCheck
    /// <summary>
    /// Executed during bootstrapping time to carry out environment tests
    /// against the application
    /// </summary>
    public interface IEnvironmentCheck
    {
        /// <summary>
        /// Asserts that the current check is valid. Throw an exception
        /// to denote a failure
        /// </summary>
        /// <param name="runtime"></param>
        void Assert(JasperRuntime runtime);

        /// <summary>
        /// A textual description for command line output that describes
        /// what is being checked
        /// </summary>
        string Description { get; }
    }
    // ENDSAMPLE
}
