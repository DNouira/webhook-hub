using Microsoft.EntityFrameworkCore;
using WebhookHub.Domain;

namespace WebhookHub.Infrastructure.Persistence;

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Event>(e =>
        {
            e.ToTable("events");
            e.HasKey(x => x.Id);
            e.Property(x => x.Source).HasMaxLength(100).IsRequired();
            e.Property(x => x.ReceivedAt);
        });
    }
}
