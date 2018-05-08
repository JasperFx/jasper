using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.SqlServer;
using Jasper.SqlServer.Persistence;
using Newtonsoft.Json;
using TestMessages;

namespace SqlSender
{
    public class HomeEndpoint
    {
        private static string _json1;
        private static string _json2;


        static HomeEndpoint()
        {
            _json1 = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(HomeEndpoint), "target1.json").ReadAllText();
            _json2 = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(HomeEndpoint), "target2.json").ReadAllText();
        }


        public static string Index(JasperRuntime runtime)
        {
            var writer = new StringWriter();
            runtime.Describe(writer);

            return writer.ToString();
        }

        public static void post_clear(SqlServerEnvelopePersistor persistor)
        {
            persistor.ClearAllStoredMessages();

        }

        [SqlTransaction]
        public static async Task post_one(IMessageContext context, SqlTransaction tx)
        {
            var target1 = JsonConvert.DeserializeObject<Target>(_json1);
            target1.Id = Guid.NewGuid();

            await tx.StoreSent(target1.Id, "Target");

            await context.Send(target1);
        }

        [SqlTransaction]
        public static async Task post_two(IMessageContext context, SqlTransaction tx)
        {
            var target2 = JsonConvert.DeserializeObject<Target>(_json2);
            target2.Id = Guid.NewGuid();

            await tx.StoreSent(target2.Id, "Target");

            await context.Send(target2);
        }

        [SqlTransaction]
        public static async Task post_three(IMessageContext context, SqlTransaction tx)
        {
            var ping = new PingMessage
            {
                Name = "Han Solo",
                Id = Guid.NewGuid()
            };

            await tx.StoreSent(ping.Id, "PingMessage");

            await context.SendAndExpectResponseFor<PongMessage>(ping);
        }

        [SqlTransaction]
        public static async Task post_four(IMessageContext context, SqlTransaction tx)
        {
            var created = new UserCreated
            {
                Id = Guid.NewGuid(),
                UserId = "Chewbacca"
            };

            await tx.StoreSent(created.Id, "UserCreated");

            await context.Send(created);
        }
    }
}
