using Microsoft.EntityFrameworkCore;
using PenielPlaceMinistriesCSharp.Models;
using System.Reflection.Emit;

namespace PenielPlaceMinistriesCSharp.Data;

public class AppDbContext(
    DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<Receipt> Receipts => Set<Receipt>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();

        b.Entity<User>()
            .Property(x => x.Email)
            .UseCollation("NOCASE");

        b.Entity<Donor>()
            .HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Donation>()
            .HasOne(x => x.Donor)
            .WithMany(x => x.Donations)
            .HasForeignKey(x => x.DonorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Donation>()
            .HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Donation>()
            .HasOne(x => x.Receipt)
            .WithOne(x => x.Donation)
            .HasForeignKey<Receipt>(x => x.DonationId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Receipt>()
            .HasIndex(x => x.DonationId)
            .IsUnique();

        b.Entity<Receipt>()
            .HasIndex(x => x.ReceiptNumber)
            .IsUnique();

        b.Entity<Donation>()
            .Property(x => x.AmountCents)
            .IsRequired(false);
    }
}

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();

        var hasher = sp.GetRequiredService<
            Microsoft.AspNetCore.Identity.IPasswordHasher<User>>();

        if (await db.Users.AnyAsync())
            return;

        var admin = new User
        {
            Name = "Ministry Administrator",
            Email = "admin@peniel.local",
            Role = "admin"
        };

        admin.PasswordHash = hasher.HashPassword(
            admin, "ChangeMe123!");

        var volunteer = new User
        {
            Name = "Sample Volunteer",
            Email = "volunteer@peniel.local",
            Role = "volunteer"
        };

        volunteer.PasswordHash = hasher.HashPassword(
            volunteer, "Volunteer123!");

        db.Users.AddRange(admin, volunteer);

        await db.SaveChangesAsync();
    }
}