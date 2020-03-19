using System;

namespace TestingSupport.Compliance
{
    public class NewUser
    {
        public string UserId { get; set; }
    }

    public class EditUser
    {
    }

    public class DeleteUser
    {
        public int Number1;
        public int Number2;
        public int Number3;
    }

    // SAMPLE: PingAndPongMessage
    public class PingMessage
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class PongMessage
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    // ENDSAMPLE


    public class UserCreated
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
    }

    public class UserDeleted
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
    }

    public class SentTrack
    {
        public Guid Id { get; set; }
        public string MessageType { get; set; }
    }

    public class ReceivedTrack
    {
        public Guid Id { get; set; }
        public string MessageType { get; set; }
    }



    public class TimeoutsMessage
    {
    }

    public class ExecutedMessage
    {
    }

    public class ExecutedMessageGuy
    {
        public static void Handle(ExecutedMessage message)
        {
        }
    }
}
