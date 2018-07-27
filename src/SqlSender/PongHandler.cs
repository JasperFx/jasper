﻿using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Persistence.SqlServer;
using TestMessages;

namespace SqlSender
{
    public class PongHandler
    {
        [SqlTransaction]
        public Task Handle(PongMessage message, SqlTransaction tx)
        {
            return tx.StoreReceived(message.Id, "PongMessage");
        }
    }
}
