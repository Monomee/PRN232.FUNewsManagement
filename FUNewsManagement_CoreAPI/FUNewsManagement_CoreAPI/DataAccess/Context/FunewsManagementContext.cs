using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;

namespace HoangNV_SE1930_A01_BE.DataAccess.Context;

public partial class FunewsManagementContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public FunewsManagementContext()
    {
    }

    public FunewsManagementContext(DbContextOptions<FunewsManagementContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<NewsArticle> NewsArticles { get; set; }
    public virtual DbSet<SystemAccount> SystemAccounts { get; set; }
    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var connectionString = config.GetConnectionString("MyCnn");
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryDesciption).HasMaxLength(250);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.ParentCategoryId).HasColumnName("ParentCategoryID");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Category_Category");
        });

        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.ToTable("NewsArticle");

            entity.Property(e => e.NewsArticleId)
                .HasMaxLength(20)
                .HasColumnName("NewsArticleID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedById).HasColumnName("CreatedByID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Headline).HasMaxLength(150);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NewsContent).HasMaxLength(4000);
            entity.Property(e => e.NewsSource).HasMaxLength(400);
            entity.Property(e => e.NewsTitle).HasMaxLength(400);
            entity.Property(e => e.UpdatedById).HasColumnName("UpdatedByID");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);

            entity.HasOne(d => d.Category).WithMany(p => p.NewsArticles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_NewsArticle_Category");

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.NewsArticles)
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_NewsArticle_SystemAccount");

            entity.HasMany(d => d.Tags).WithMany(p => p.NewsArticles)
                .UsingEntity<Dictionary<string, object>>(
                    "NewsTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NewsTag_Tag"),
                    l => l.HasOne<NewsArticle>().WithMany()
                        .HasForeignKey("NewsArticleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NewsTag_NewsArticle"),
                    j =>
                    {
                        j.HasKey("NewsArticleId", "TagId");
                        j.ToTable("NewsTag");
                        j.IndexerProperty<string>("NewsArticleId")
                            .HasMaxLength(20)
                            .HasColumnName("NewsArticleID");
                        j.IndexerProperty<int>("TagId").HasColumnName("TagID");
                    });
        });

        modelBuilder.Entity<SystemAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId);

            entity.ToTable("SystemAccount");

            entity.Property(e => e.AccountId)
                .ValueGeneratedNever()
                .HasColumnName("AccountID");
            entity.Property(e => e.AccountEmail).HasMaxLength(70);
            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.AccountPassword).HasMaxLength(70);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK_HashTag");

            entity.ToTable("Tag");

            entity.Property(e => e.TagId)
                .ValueGeneratedNever()
                .HasColumnName("TagID");
            entity.Property(e => e.Note).HasMaxLength(400);
            entity.Property(e => e.TagName).HasMaxLength(50);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("RefreshToken");
            entity.Property(e => e.Token).HasMaxLength(200);
            entity.Property(e => e.JwtId).HasMaxLength(100);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RefreshToken_SystemAccount");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("AuditLog");
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.Timestamp).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        if (auditEntries.Count > 0)
        {
            await OnAfterSaveChanges(auditEntries, cancellationToken);
        }
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        string currentUserId = "Anonymous";
        var user = _httpContextAccessor?.HttpContext?.User;
        if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
        {
            currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier) 
                            ?? user.FindFirstValue(ClaimTypes.Email) 
                            ?? user.Identity.Name 
                            ?? "AuthenticatedUser";
        }

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.Entity is RefreshToken || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            {
                continue;
            }

            var auditEntry = new AuditEntry(entry)
            {
                EntityName = entry.Metadata.ClrType.Name,
                UserId = currentUserId,
                Timestamp = DateTime.UtcNow
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.Action = "Create";
                    foreach (var property in entry.Properties)
                    {
                        auditEntry.AfterValues[property.Metadata.Name] = property.CurrentValue;
                    }
                    break;

                case EntityState.Deleted:
                    auditEntry.Action = "Delete";
                    foreach (var property in entry.Properties)
                    {
                        auditEntry.BeforeValues[property.Metadata.Name] = property.OriginalValue;
                    }
                    break;

                case EntityState.Modified:
                    auditEntry.Action = "Update";
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified)
                        {
                            auditEntry.BeforeValues[property.Metadata.Name] = property.OriginalValue;
                            auditEntry.AfterValues[property.Metadata.Name] = property.CurrentValue;
                        }
                    }
                    break;
            }

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }

    private static readonly JsonSerializerOptions AuditJsonOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        foreach (var entry in auditEntries)
        {
            AuditLogs.Add(new AuditLog
            {
                UserId = entry.UserId,
                Action = entry.Action,
                EntityName = entry.EntityName,
                Timestamp = entry.Timestamp,
                BeforeJson = entry.BeforeValues.Count > 0 ? JsonSerializer.Serialize(entry.BeforeValues, AuditJsonOptions) : null,
                AfterJson = entry.AfterValues.Count > 0 ? JsonSerializer.Serialize(entry.AfterValues, AuditJsonOptions) : null
            });
        }

        await base.SaveChangesAsync(cancellationToken);
    }
}

internal class AuditEntry
{
    public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
    public string UserId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object?> BeforeValues { get; } = new();
    public Dictionary<string, object?> AfterValues { get; } = new();

    public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        Entry = entry;
    }
}
