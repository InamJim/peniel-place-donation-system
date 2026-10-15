using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PenielPlaceMinistriesCSharp.Data;
using PenielPlaceMinistriesCSharp.Models;

namespace PenielPlaceMinistriesCSharp.Services;

public interface IAuthService
{
    Task<User?> ValidateAsync(string email, string password);
}

public interface IDonorService
{
    Task<List<Donor>> AllAsync();
    Task<Donor?> FindAsync(int id);
    Task<Donor> CreateAsync(DonorRequest request, int userId);
}

public interface IDonationService
{
    Task<List<Donation>> AllAsync(
        DateOnly? from,
        DateOnly? to,
        string? category,
        int? donorId);

    Task<Donation> CreateAsync(DonationRequest request, int userId);
    Task<Donation?> FindAsync(int id);
}

public interface IReceiptService
{
    Task<Donation?> GetForDonationAsync(int donationId);
}

public interface IReportingService
{
    Task<ReportResult> ReportAsync(
        DateOnly? from,
        DateOnly? to,
        string? category,
        int? donorId);
}

// Authentication and password verification.
public sealed class AuthService(
    AppDbContext db,
    IPasswordHasher<User> hasher) : IAuthService
{
    public async Task<User?> ValidateAsync(
        string email,
        string password)
    {
        var user = await db.Users.SingleOrDefaultAsync(
            u => u.Email == email.Trim());

        if (user is null)
            return null;

        var result = hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, password);
        }

        await db.SaveChangesAsync();

        return user;
    }
}

// Donor management.
public sealed class DonorService(AppDbContext db) : IDonorService
{
    public Task<List<Donor>> AllAsync() =>
        db.Donors
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

    public Task<Donor?> FindAsync(int id) =>
        db.Donors
            .Include(x => x.Donations)
            .ThenInclude(x => x.Receipt)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

    public async Task<Donor> CreateAsync(
        DonorRequest request,
        int userId)
    {
        var name = request.Name.Trim();

        if (name.Length == 0)
            throw new ArgumentException("Donor name is required.");

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            !new System.ComponentModel.DataAnnotations
                .EmailAddressAttribute()
                .IsValid(request.Email))
        {
            throw new ArgumentException(
                "Enter a valid email address.");
        }

        var donor = new Donor
        {
            Name = name,
            Email = Clean(request.Email),
            Phone = Clean(request.Phone),
            Notes = Clean(request.Notes),
            CreatedById = userId
        };

        db.Donors.Add(donor);
        await db.SaveChangesAsync();

        return donor;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

// Donation recording, filtering, validation, and receipt creation.
public sealed class DonationService(AppDbContext db) : IDonationService
{
    public async Task<List<Donation>> AllAsync(
        DateOnly? from,
        DateOnly? to,
        string? category,
        int? donorId)
    {
        ValidateFilters(from, to, category, donorId);

        var query = db.Donations
            .Include(x => x.Donor)
            .Include(x => x.Receipt)
            .AsNoTracking()
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(x => x.ReceivedOn >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.ReceivedOn <= to.Value);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category == category);

        if (donorId.HasValue)
            query = query.Where(x => x.DonorId == donorId.Value);

        return await query
            .OrderByDescending(x => x.ReceivedOn)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<Donation> CreateAsync(
        DonationRequest request,
        int userId)
    {
        var category = request.Category.Trim();
        var kind = request.Kind.Trim().ToLowerInvariant();

        if (!DonationCategories.All.Contains(category))
        {
            throw new ArgumentException(
                "Select a valid donation category.");
        }

        if (kind is not ("monetary" or "in_kind"))
        {
            throw new ArgumentException(
                "Donation kind must be monetary or in-kind.");
        }

        if ((category == DonationCategories.InKind) !=
            (kind == "in_kind"))
        {
            throw new ArgumentException(
                "In-kind category and donation kind must match.");
        }

        if (!request.ReceivedOn.HasValue)
        {
            throw new ArgumentException(
                "Donation date is required.");
        }

        if (request.ReceivedOn.Value >
            DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentException(
                "Donation date cannot be in the future.");
        }

        if (request.DonorId.HasValue &&
            !await db.Donors.AnyAsync(
                x => x.Id == request.DonorId.Value))
        {
            throw new ArgumentException(
                "Selected donor does not exist.");
        }

        long? cents = null;

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        if (kind == "monetary")
        {
            if (!request.Amount.HasValue ||
                request.Amount.Value <= 0)
            {
                throw new ArgumentException(
                    "Monetary amount must be greater than zero.");
            }

            if (decimal.Round(request.Amount.Value, 2) !=
                request.Amount.Value)
            {
                throw new ArgumentException(
                    "Amount may have at most two decimal places.");
            }

            if (description is not null)
            {
                throw new ArgumentException(
                    "Description is only for in-kind donations.");
            }

            cents = checked(
                (long)(request.Amount.Value * 100m));
        }
        else if (description is null)
        {
            throw new ArgumentException(
                "Describe the in-kind donation, e.g. groceries or clothing.");
        }

        // Save the donation and its receipt in one transaction.
        // If either save fails, roll back both operations.
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        try
        {
            var donation = new Donation
            {
                DonorId = request.DonorId,
                Category = category,
                Kind = kind,
                AmountCents = cents,
                Description = description,
                ReceivedOn = request.ReceivedOn.Value,
                Notes = string.IsNullOrWhiteSpace(request.Notes)
                    ? null
                    : request.Notes.Trim(),
                CreatedById = userId
            };

            db.Donations.Add(donation);
            await db.SaveChangesAsync();

            var receipt = new Receipt
            {
                DonationId = donation.Id,
                ReceiptNumber =
                    $"PPM-{DateTime.UtcNow:yyyyMMdd}-{donation.Id:D6}"
            };

            db.Receipts.Add(receipt);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();

            donation.Receipt = receipt;

            return donation;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<Donation?> FindAsync(int id) =>
        db.Donations
            .Include(x => x.Donor)
            .Include(x => x.Receipt)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

    private static void ValidateFilters(
        DateOnly? from,
        DateOnly? to,
        string? category,
        int? donorId)
    {
        if (from.HasValue && to.HasValue && from > to)
        {
            throw new ArgumentException(
                "Start date cannot be after end date.");
        }

        if (!string.IsNullOrWhiteSpace(category) &&
            !DonationCategories.All.Contains(category))
        {
            throw new ArgumentException(
                "Invalid donation category filter.");
        }

        if (donorId.HasValue && donorId.Value < 1)
        {
            throw new ArgumentException(
                "Invalid donor filter.");
        }
    }
}

// Receipt retrieval.
public sealed class ReceiptService(
    IDonationService donations) : IReceiptService
{
    public Task<Donation?> GetForDonationAsync(int donationId) =>
        donationId < 1
            ? Task.FromResult<Donation?>(null)
            : donations.FindAsync(donationId);
}

// Reporting result and reporting calculations.
public sealed record ReportResult(
    List<Donation> Donations,
    long TotalCents,
    int MonetaryCount,
    int InKindCount,
    Dictionary<string, long> TotalsByCategory);

public sealed class ReportingService(
    IDonationService donations) : IReportingService
{
    public async Task<ReportResult> ReportAsync(
        DateOnly? from,
        DateOnly? to,
        string? category,
        int? donorId)
    {
        var rows = await donations.AllAsync(
            from, to, category, donorId);

        var monetary = rows
            .Where(x => x.Kind == "monetary")
            .ToList();

        var totals = monetary
            .GroupBy(x => x.Category)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(x => x.AmountCents ?? 0));

        return new ReportResult(
            rows,
            monetary.Sum(x => x.AmountCents ?? 0),
            monetary.Count,
            rows.Count(x => x.Kind == "in_kind"),
            totals);
    }
}