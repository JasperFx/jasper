namespace TestMessages
{
    public class NewUser
    {
        public string UserId { get; set; }
    }
    public class EditUser{}

    public class DeleteUser
    {
        public int Number1;
        public int Number2;
        public int Number3;
    }

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

}
