using Jasper.SqlServer.Schema;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests
{
    public class SchemaLoaderTests
    {
        [Fact]
        public void retrieve_creation_script()
        {
            var sql = SchemaLoader.ToCreationScript("foo");

            sql.ShouldContain("create table foo.jasper_outgoing_envelopes");
            sql.ShouldContain("create table foo.jasper_incoming_envelopes");
        }

        [Fact]
        public void retrieve_drop_script()
        {
            var sql = SchemaLoader.ToDropScript("foo");

            sql.ShouldContain("drop table foo.jasper_outgoing_envelopes");
            sql.ShouldContain("drop table foo.jasper_incoming_envelopes");
        }

        [Fact]
        public void drop_then_create()
        {
            var loader = new SchemaLoader(ConnectionSource.ConnectionString);
            loader.DropAll();

            loader.CreateAll();
        }

        [Fact]
        public void recreate_all_tables()
        {
            var loader = new SchemaLoader(ConnectionSource.ConnectionString);
            loader.RecreateAll();
        }
    }
}
