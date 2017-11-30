namespace Jasper.Diagnostics.Messages
{
    public class RequestDiagnosticData : ClientMessage
    {
        public RequestDiagnosticData(): base("diagnostics-request-data"){}
    }
}
