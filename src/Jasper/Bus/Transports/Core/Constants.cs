using System.Text;

namespace Jasper.Bus.Transports.Core
{
    public static class Constants
    {
        // The first four values are the possible receive confirmation messages.
        // NOTE: "Recieved" is misspelled intentionally to preserve compatibility.
        // The original Rhino Queues protocol used this spelling: https://github.com/hibernating-rhinos/rhino-queues/blob/master/Rhino.Queues/Protocol/ProtocolConstants.cs
        public const string Received = "Recieved";
        public const string SerializationFailure = "FailDesr";
        public const string ProcessingFailure = "FailPrcs";
        public const string QueueDoesNotExist = "Qu-Exist";

        public const string Acknowledged = "Acknowledged";
        public const string Revert = "Revert";

        public static byte[] ReceivedBuffer = Encoding.Unicode.GetBytes(Received);
        public static byte[] SerializationFailureBuffer = Encoding.Unicode.GetBytes(SerializationFailure);
        public static byte[] ProcessingFailureBuffer = Encoding.Unicode.GetBytes(ProcessingFailure);
        public static byte[] QueueDoesNotExistBuffer = Encoding.Unicode.GetBytes(QueueDoesNotExist);

        public static byte[] AcknowledgedBuffer = Encoding.Unicode.GetBytes(Acknowledged);
        public static byte[] RevertBuffer = Encoding.Unicode.GetBytes(Revert);
    }
}
