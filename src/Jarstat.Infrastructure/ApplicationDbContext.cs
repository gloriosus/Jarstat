using Microsoft.EntityFrameworkCore;
using Jarstat.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Jarstat.Domain.Records;

namespace Jarstat.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Domain.Entities.File> Files => Set<Domain.Entities.File>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Item> Items => Set<Item>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Document>(document =>
        {
            document.HasIndex(d => new { d.FileName, d.FolderId })
                    .IsUnique();

            document.HasOne(d => d.Folder)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

            document.HasOne(d => d.Creator)
                    .WithMany()
                    .OnDelete(DeleteBehavior.SetNull);

            document.HasOne(d => d.LastUpdater)
                    .WithMany()
                    .OnDelete(DeleteBehavior.SetNull);

            document.HasOne(d => d.File)
                    .WithOne()
                    .HasForeignKey(typeof(Document), "FileId")
                    .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Folder>(folder =>
        {
            folder.HasIndex(f => new { f.DisplayName, f.ParentId })
                  .IsUnique();

            folder.HasOne(f => f.Parent)
                  .WithMany()
                  .OnDelete(DeleteBehavior.Cascade);

            folder.HasOne(f => f.Creator)
                  .WithMany()
                  .OnDelete(DeleteBehavior.SetNull);

            folder.HasOne(f => f.LastUpdater)
                  .WithMany()
                  .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<User>(user =>
        {
            user.HasOne(u => u.Creator)
                .WithMany()
                .HasForeignKey("CreatorId")
                .OnDelete(DeleteBehavior.SetNull);

            user.HasOne(u => u.LastUpdater)
                .WithMany()
                .HasForeignKey("LastUpdaterId")
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        optionsBuilder.UseNpgsql(configuration.GetConnectionString("ApplicationDbContextConnection"));
    }
}
