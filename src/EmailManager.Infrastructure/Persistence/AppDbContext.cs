using EmailManager.Domain;
using EmailManager.Domain.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmailManager.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Mailbox> Mailboxes => Set<Mailbox>();
    public DbSet<EmailMessage> Messages => Set<EmailMessage>();
    public DbSet<EmailAttachment> Attachments => Set<EmailAttachment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Mailbox>().HasIndex(m => m.Address).IsUnique();
    }
}