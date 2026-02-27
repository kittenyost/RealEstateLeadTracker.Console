using Microsoft.EntityFrameworkCore;

namespace RealEstateLeadTracker.Console.EfCore.Context;

public partial class ProjectDbContext : DbContext
{
    public ProjectDbContext()
    {
    }

    public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
        : base(options)
    {
    }

    // ✅ Fully-qualified DbSet types (prevents EF from picking the domain Lead)
    public virtual DbSet<RealEstateLeadTracker.Console.EfCore.Entities.Lead> Leads { get; set; }
    public virtual DbSet<RealEstateLeadTracker.Console.EfCore.Entities.LeadNote> LeadNotes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(
            "Server=LAPTOP-KATHY;Database=RealEstateLeadTracker;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ✅ Fully-qualified entity mapping
        modelBuilder.Entity<RealEstateLeadTracker.Console.EfCore.Entities.Lead>(entity =>
        {
            entity.HasKey(e => e.LeadId).HasName("PK__Leads__73EF78FA9559F614");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<RealEstateLeadTracker.Console.EfCore.Entities.LeadNote>(entity =>
        {
            entity.HasKey(e => e.LeadNoteId).HasName("PK__LeadNote__C8B0B7B6F7A87B17");
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Lead)
                .WithMany(p => p.LeadNotes)
                .HasForeignKey(d => d.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // If anything else is overriding mappings, it happens here:
        OnModelCreatingPartial(modelBuilder);

        // ✅ FINAL OVERRIDE (wins last) - fully-qualified
        modelBuilder.Entity<RealEstateLeadTracker.Console.EfCore.Entities.Lead>().ToTable("Leads");
        modelBuilder.Entity<RealEstateLeadTracker.Console.EfCore.Entities.LeadNote>().ToTable("LeadNotes");
    }

    partial void OnModelCreatingPartial(ModelBuilder mmodelBuilder);
}