using System.Threading.Tasks;

namespace benchmarks
{
    public class UserCreatedHandler
    {
        private readonly IDatabase _database;
        private readonly IService3 _service3;

        public UserCreatedHandler(IDatabase database, IService3 service3)
        {
            _database = database;
            _service3 = service3;
        }

        public Task Handle(UserCreated created)
        {
            return _database.Save(created);
        }
    }
}
