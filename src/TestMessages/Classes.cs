namespace TestMessages
{
    public class NewUser{}
    public class EditUser{}
    public class DeleteUser{}

    public class PingMessage
    {
        public string Name { get; set; }
    }

    public class PongMessage
    {
        public string Name { get; set; }
    }


    public class UserCreated
    {
        public string UserId { get; set; }
    }

    public class UserDeleted
    {
        public string UserId { get; set; }
    }

    public class BeginUserRegistration
    {
        public string UserId { get; set; }
    }


}
