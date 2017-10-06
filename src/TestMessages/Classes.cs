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


    public class UserCreated{}
    public class UserDeleted{}


}
