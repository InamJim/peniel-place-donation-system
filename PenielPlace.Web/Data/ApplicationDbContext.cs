using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PenielPlace.Web.Models;

namespace PenielPlace.Web.Data;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<DonationCategory> DonationCategories => Set<DonationCategory>();
    public DbSet<DonationRecord> Donations => Set<DonationRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Donor>(entity =>
        {
            entity.HasKey(x => x.DonorId);
            entity.Property(x => x.FullNameProtected).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.EmailProtected).HasMaxLength(1024);
            entity.Property(x => x.PhoneProtected).HasMaxLength(512);
            entity.Property(x => x.NotesProtected).HasMaxLength(2048);
        });

        builder.Entity<DonationCategory>(entity =>
        {
            entity.HasKey(x => x.DonationCategoryId);
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasData(
                new DonationCategory { DonationCategoryId = 1, Name = "Tithe", IsInKind = false, IsActive = true },
                new DonationCategory { DonationCategoryId = 2, Name = "Offering", IsInKind = false, IsActive = true },
                new DonationCategory { DonationCategoryId = 3, Name = "Once-off gift", IsInKind = false, IsActive = true },
                new DonationCategory { DonationCategoryId = 4, Name = "In kind", IsInKind = true, IsActive = true });
        });

        builder.Entity<DonationRecord>(entity =>
        {
            entity.HasKey(x => x.DonationId);
            entity.Property(x => x.AmountProtected).HasMaxLength(256);
            entity.Property(x => x.InKindDescriptionProtected).HasMaxLength(1024);
            entity.Property(x => x.PaymentMethodProtected).HasMaxLength(256);
            entity.Property(x => x.NotesProtected).HasMaxLength(2048);
            entity.Property(x => x.ReceiptNumber).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.ReceiptNumber).IsUnique();
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Donations)
                .HasForeignKey(x => x.DonationCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Donor)
                .WithMany(x => x.Donations)
                .HasForeignKey(x => x.DonorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
