using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Weasel.Core.Migrations;

namespace Jasper.Persistence.Database
{
    internal class MigrationLogger : IMigrationLogger
    {
        private readonly ILogger _logger;

        public MigrationLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void SchemaChange(string sql)
        {
            _logger.LogInformation("Applied database migration for Jasper Envelope Storage: {SQL}", sql);
        }

        public void OnFailure(DbCommand command, Exception ex)
        {
            _logger.LogError(ex, "Error executing Jasper Envelope Storage database migration: {SQL}", command.CommandText);
        }
    }
}
