using Microsoft.EntityFrameworkCore;
using SetlistSocial.Api.Models;

namespace SetlistSocial.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Concert> Concerts => Set<Concert>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<ActivityEvent> ActivityEvents => Set<ActivityEvent>();
    public DbSet<Tag> Tags => Set<Tag>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasIndex(user => user.OAuthSubject).IsUnique();

            entity.Property(user => user.DisplayName)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(user => user.OAuthSubject)
                .HasMaxLength(200)
                .IsRequired();
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.Property(artist => artist.Name)
                .HasMaxLength(200)
                .IsRequired();
        });

        modelBuilder.Entity<Concert>(entity =>
        {
            entity.Property(concert => concert.Title)
                .HasMaxLength(240)
                .IsRequired();

            entity.Property(concert => concert.VenueName)
                .HasMaxLength(200);

            entity.Property(concert => concert.City)
                .HasMaxLength(120);

            entity.Property(concert => concert.Region)
                .HasMaxLength(120);

            entity.Property(concert => concert.Country)
                .HasMaxLength(120);

            entity.HasOne(concert => concert.UserProfile)
                .WithMany(user => user.Concerts)
                .HasForeignKey(concert => concert.UserProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(concert => concert.Artist)
                .WithMany(artist => artist.Concerts)
                .HasForeignKey(concert => concert.ArtistId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(concert => concert.Tags)
                .WithMany(tag => tag.Concerts)
                .UsingEntity("ConcertTag");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(review => review.Body)
                .HasMaxLength(2000);

            entity.HasOne(review => review.UserProfile)
                .WithMany(user => user.Reviews)
                .HasForeignKey(review => review.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(review => review.Concert)
                .WithMany(concert => concert.Reviews)
                .HasForeignKey(review => review.ConcertId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.Property(item => item.Notes)
                .HasMaxLength(1000);

            entity.Property(item => item.SourceName)
                .HasMaxLength(120);

            entity.Property(item => item.SourceUrl)
                .HasMaxLength(1000);

            entity.HasOne(item => item.UserProfile)
                .WithMany(user => user.WishlistItems)
                .HasForeignKey(item => item.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Artist)
                .WithMany()
                .HasForeignKey(item => item.ArtistId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ActivityEvent>(entity =>
        {
            entity.Property(activity => activity.EventType)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(activity => activity.Summary)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasOne(activity => activity.UserProfile)
                .WithMany(user => user.ActivityEvents)
                .HasForeignKey(activity => activity.UserProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(activity => activity.Concert)
                .WithMany()
                .HasForeignKey(activity => activity.ConcertId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(tag => tag.Name).IsUnique();

            entity.Property(tag => tag.Name)
                .HasMaxLength(80)
                .IsRequired();
        });
    }

    private void ApplyTimestamps()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IHasTimestamps>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
