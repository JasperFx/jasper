using System.Threading.Tasks;

namespace benchmarks
{
    public class UserCreatedEndpoint
    {
        private readonly IDatabase _database;
        private readonly IService3 _service3;

        public UserCreatedEndpoint(IDatabase database, IService3 service3)
        {
            _database = database;
            _service3 = service3;
        }

        public Task post_user_created(UserCreated created)
        {
            return _database.Save(created);
        }
    }
}
