using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Util;

namespace Jasper.Persistence.Database
{
    public static class ReaderExtensions
    {
        public static async Task<Uri> ReadUri(this DbDataReader reader, int index, CancellationToken cancellation = default)
        {
            if (await reader.IsDBNullAsync(index, cancellation))
            {
                return default;
            }

            return (await reader.GetFieldValueAsync<string>(index, cancellation)).ToUri();
        }

        public static async Task<T> MaybeRead<T>(this DbDataReader reader, int index,
            CancellationToken cancellation = default)
        {
            if (await reader.IsDBNullAsync(index, cancellation))
            {
                return default;
            }

            return await reader.GetFieldValueAsync<T>(index, cancellation);
        }
    }
}
