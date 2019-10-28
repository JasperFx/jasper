using Jasper.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jasper.Persistence.Testing.EFCore
{
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.MapEnvelopeStorage();
        }


    }
}
