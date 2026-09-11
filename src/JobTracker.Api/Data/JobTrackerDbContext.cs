using JobTracker.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Data;

public class JobTrackerDbContext(DbContextOptions<JobTrackerDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobApplication> Applications => Set<JobApplication>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<StatusEvent> StatusEvents => Set<StatusEvent>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Company>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Industry).HasMaxLength(100);
            e.Property(c => c.Website).HasMaxLength(500);
            e.Property(c => c.Location).HasMaxLength(200);
            e.Property(c => c.Notes).HasMaxLength(2000);
            e.HasIndex(c => c.Name).IsUnique();
        });

        model.Entity<JobApplication>(e =>
        {
            e.Property(a => a.RoleTitle).IsRequired().HasMaxLength(200);
            e.Property(a => a.JobUrl).HasMaxLength(1000);
            e.Property(a => a.Source).HasMaxLength(100);
            e.Property(a => a.Location).HasMaxLength(200);
            e.Property(a => a.Notes).HasMaxLength(4000);
            e.Property(a => a.SalaryMin).HasPrecision(12, 2);
            e.Property(a => a.SalaryMax).HasPrecision(12, 2);
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

            e.HasIndex(a => a.Status);
            e.HasIndex(a => a.AppliedDate);

            e.HasOne(a => a.Company)
                .WithMany(c => c.Applications)
                .HasForeignKey(a => a.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<Interview>(e =>
        {
            e.Property(i => i.Stage).HasConversion<string>().HasMaxLength(20);
            e.Property(i => i.Outcome).HasConversion<string>().HasMaxLength(20);
            e.Property(i => i.InterviewerName).HasMaxLength(200);
            e.Property(i => i.Notes).HasMaxLength(4000);

            e.HasIndex(i => i.ScheduledAt);

            e.HasOne(i => i.Application)
                .WithMany(a => a.Interviews)
                .HasForeignKey(i => i.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<StatusEvent>(e =>
        {
            e.Property(s => s.FromStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(s => s.ToStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(s => s.Note).HasMaxLength(1000);

            e.HasIndex(s => s.ChangedAt);

            e.HasOne(s => s.Application)
                .WithMany(a => a.StatusHistory)
                .HasForeignKey(s => s.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
