using Jasper.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jasper.Persistence.Testing.EFCore.Sagas
{
    public class SagaDbContext : DbContext
    {
        private readonly DbContextOptions _options;

        public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
        {
            _options = options;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.MapEnvelopeStorage();

            modelBuilder.Entity<GuidWorkflowState>(map =>
            {

                map.ToTable("GuidWorkflowState");
                map.HasKey(x => x.Id);
                map.Property(x => x.OneCompleted).HasColumnName("one");
                map.Property(x => x.TwoCompleted).HasColumnName("two");
                map.Property(x => x.ThreeCompleted).HasColumnName("three");
                map.Property(x => x.FourCompleted).HasColumnName("four");
            });

            modelBuilder.Entity<IntWorkflowState>(map =>
            {

                map.ToTable("IntWorkflowState");
                map.HasKey(x => x.Id);
                map.Property(x => x.OneCompleted).HasColumnName("one");
                map.Property(x => x.TwoCompleted).HasColumnName("two");
                map.Property(x => x.ThreeCompleted).HasColumnName("three");
                map.Property(x => x.FourCompleted).HasColumnName("four");
            });

            modelBuilder.Entity<StringWorkflowState>(map =>
            {

                map.ToTable("StringWorkflowState");
                map.HasKey(x => x.Id);
                map.Property(x => x.OneCompleted).HasColumnName("one");
                map.Property(x => x.TwoCompleted).HasColumnName("two");
                map.Property(x => x.ThreeCompleted).HasColumnName("three");
                map.Property(x => x.FourCompleted).HasColumnName("four");
            });

            modelBuilder.Entity<LongWorkflowState>(map =>
            {
                map.ToTable("LongWorkflowState");
                map.HasKey(x => x.Id);
                map.Property(x => x.OneCompleted).HasColumnName("one");
                map.Property(x => x.TwoCompleted).HasColumnName("two");
                map.Property(x => x.ThreeCompleted).HasColumnName("three");
                map.Property(x => x.FourCompleted).HasColumnName("four");
            });
        }
    }
}
