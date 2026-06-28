using FiapGames.Notifications.Models;
using Microsoft.EntityFrameworkCore;

namespace FiapGames.Notifications.Data;

public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.HasIndex(n => n.CorrelationId);
            entity.Property(n => n.EventType).HasMaxLength(100);
            entity.Property(n => n.RecipientEmail).HasMaxLength(256);
            entity.Property(n => n.Subject).HasMaxLength(500);
        });
    }
}
