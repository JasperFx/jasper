using System;
using System.Data.SqlClient;
using Jasper.SqlServer.Util;
using Oakton;

namespace RunLoadTests
{
    [Description("Rebuild all the database objects for the Sql Server database", Name = "build-sql")]
    public class BuildSqlCommand : OaktonCommand<SqlInput>
    {
        public override bool Execute(SqlInput input)
        {
            using (var conn = new SqlConnection(input.ConnectionFlag))
            {
                conn.Open();


                conn.CreateCommand(@"

IF OBJECT_ID('receiver.sent_track', 'U') IS NOT NULL
  drop table receiver.sent_track;

IF OBJECT_ID('receiver.received_track', 'U') IS NOT NULL
  drop table receiver.received_track;

IF OBJECT_ID('sender.sent_track', 'U') IS NOT NULL
  drop table sender.sent_track;

IF OBJECT_ID('sender.received_track', 'U') IS NOT NULL
  drop table sender.received_track;



create table sender.sent_track
(
	id uniqueidentifier not null primary key,
	message_type varchar(250) not null,
);

create table sender.received_track
(
	id uniqueidentifier not null primary key,
	message_type varchar(250) not null,
);

create table receiver.sent_track
(
	id uniqueidentifier not null primary key,
	message_type varchar(250) not null,
);

create table receiver.received_track
(
	id uniqueidentifier not null primary key,
	message_type varchar(250) not null,
);

").ExecuteNonQuery();
            }

            ConsoleWriter.Write(ConsoleColor.Green, "Success!");

            return true;
        }
    }
}
