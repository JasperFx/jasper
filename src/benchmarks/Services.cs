using System.Threading.Tasks;

namespace benchmarks
{
    public interface IDatabase
    {
        Task Save(UserCreated created);
    }

    public class Database : IDatabase
    {
        public Database(IService1 one, IService2 two)
        {
        }

        public Task Save(UserCreated created)
        {
            return Task.CompletedTask;
        }
    }

    public interface IService1{}
    public class Service1 : IService1{}
    public interface IService2{}
    public class Service2 : IService2{}
    public interface IService3{}
    public class Service3 : IService3{}
    public interface IService4{}
    public class Service4 : IService4{}
    public interface IService5{}
    public class Service5 : IService5{}
    public interface IService6{}
    public class Service6 : IService6{}
    public interface IService7{}
    public class Service7 : IService7{}
    public interface IService8{}
    public class Service8 : IService8{}
    public interface IService9{}
    public class Service9 : IService9{}
    public interface IService10{}
    public class Service10 : IService10{}
    public interface IService11{}
    public class Service11 : IService11{}
    public interface IService12{}
    public class Service12 : IService12{}
    public interface IService13{}
    public class Service13 : IService13{}
    public interface IService14{}
    public class Service14 : IService14{}
    public interface IService15{}
    public class Service15 : IService15{}
    public interface IService16{}
    public class Service16 : IService16{}
    public interface IService17{}
    public class Service17 : IService17{}
    public interface IService18{}
    public class Service18 : IService18{}
    public interface IService19{}
    public class Service19 : IService19{}
    public interface IService20{}
    public class Service20 : IService20{}
    public interface IService21{}
    public class Service21 : IService21{}
    public interface IService22{}
    public class Service22 : IService22{}
    public interface IService23{}
    public class Service23 : IService23{}
    public interface IService24{}
    public class Service24 : IService24{}
    public interface IService25{}
    public class Service25 : IService25{}
    public interface IService26{}
    public class Service26 : IService26{}
    public interface IService27{}
    public class Service27 : IService27{}
    public interface IService28{}
    public class Service28 : IService28{}
    public interface IService29{}
    public class Service29 : IService29{}
    public interface IService30{}
    public class Service30 : IService30{}
    public interface IService31{}
    public class Service31 : IService31{}
    public interface IService32{}
    public class Service32 : IService32{}
    public interface IService33{}
    public class Service33 : IService33{}
    public interface IService34{}
    public class Service34 : IService34{}

}
