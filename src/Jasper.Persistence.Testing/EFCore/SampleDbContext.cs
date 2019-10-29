using Jasper.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jasper.Persistence.Testing.EFCore
{
    public class SampleDbContext : DbContext
    {
        private readonly DbContextOptions<SampleDbContext> _options;

        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options)
        {
            _options = options;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.MapEnvelopeStorage();
        }


    }
}
