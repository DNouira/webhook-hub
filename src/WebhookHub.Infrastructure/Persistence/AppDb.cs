using Microsoft.EntityFrameworkCore;
using WebhookHub.Domain;

namespace WebhookHub.Infrastructure.Persistence;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Event>(e =>
        {
            e.ToTable("events");       // <= matches the INSERT target
            e.HasKey(x => x.Id);
            e.Property(x => x.Source).HasMaxLength(200);
        });
    }
}
