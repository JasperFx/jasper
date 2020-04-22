using Jasper.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InMemoryMediator.Items
{
    // SAMPLE: ItemsDbContext
    public class ItemsDbContext : DbContext
    {
        public ItemsDbContext(DbContextOptions<ItemsDbContext> options) : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This adds the Jasper Envelope storage mapping to
            // this DbContext type. This enables the EF Core "outbox"
            // support with Jasper
            modelBuilder.MapEnvelopeStorage();

            // Your normal EF Core mapping
            modelBuilder.Entity<Item>(map =>
            {
                map.ToTable("items");
                map.HasKey(x => x.Id);
                map.Property(x => x.Name);
            });

        }
    }
    // ENDSAMPLE
}
