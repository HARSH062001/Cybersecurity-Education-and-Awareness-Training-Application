using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<SectionProgress> SectionProgressEntries =>
        Set<SectionProgress>();

    public DbSet<AssessmentBank> AssessmentBanks =>
        Set<AssessmentBank>();

    public DbSet<AssessmentQuestion> AssessmentQuestions =>
        Set<AssessmentQuestion>();

    public DbSet<AssessmentOption> AssessmentOptions =>
        Set<AssessmentOption>();

    public DbSet<AssessmentAttempt> AssessmentAttempts =>
        Set<AssessmentAttempt>();

    public DbSet<AssessmentAnswer> AssessmentAnswers =>
        Set<AssessmentAnswer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SectionProgress>(entity =>
        {
            entity.HasIndex(progress => new
            {
                progress.UserId,
                progress.ModuleId,
                progress.SectionKey
            }).IsUnique();

            entity.HasOne(progress => progress.User)
                .WithMany()
                .HasForeignKey(progress => progress.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AssessmentBank>(entity =>
        {
            entity.HasIndex(bank => new
            {
                bank.ModuleId,
                bank.Code,
                bank.Version
            }).IsUnique();
        });

        builder.Entity<AssessmentQuestion>(entity =>
        {
            entity.HasIndex(question => new
            {
                question.AssessmentBankId,
                question.Number
            }).IsUnique();

            entity.HasOne(question => question.Bank)
                .WithMany(bank => bank.Questions)
                .HasForeignKey(question => question.AssessmentBankId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AssessmentOption>(entity =>
        {
            entity.HasIndex(option => new
            {
                option.AssessmentQuestionId,
                option.Code
            }).IsUnique();

            entity.HasOne(option => option.Question)
                .WithMany(question => question.Options)
                .HasForeignKey(option => option.AssessmentQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AssessmentAttempt>(entity =>
        {
            entity.HasIndex(attempt => new
            {
                attempt.UserId,
                attempt.StartedAtUtc
            });

            entity.Property(attempt => attempt.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(attempt => attempt.User)
                .WithMany()
                .HasForeignKey(attempt => attempt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // A bank used by an attempt must remain available for history.
            entity.HasOne(attempt => attempt.Bank)
                .WithMany()
                .HasForeignKey(attempt => attempt.AssessmentBankId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AssessmentAnswer>(entity =>
        {
            entity.HasIndex(answer => new
            {
                answer.AssessmentAttemptId,
                answer.AssessmentQuestionId
            }).IsUnique();

            entity.HasOne(answer => answer.Attempt)
                .WithMany(attempt => attempt.Answers)
                .HasForeignKey(answer => answer.AssessmentAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(answer => answer.Question)
                .WithMany()
                .HasForeignKey(answer => answer.AssessmentQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}