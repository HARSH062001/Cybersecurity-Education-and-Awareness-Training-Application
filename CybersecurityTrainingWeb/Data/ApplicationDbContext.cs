using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingWeb.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
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

    // Stores the modules assigned to each module owner.
    public DbSet<ModuleOwnerAssignment> ModuleOwnerAssignments =>
        Set<ModuleOwnerAssignment>();

    // Stores the modules each learner has enrolled in.
    public DbSet<ModuleEnrollment> ModuleEnrollments =>
        Set<ModuleEnrollment>();

    public DbSet<ModuleSchedule> ModuleSchedules => Set<ModuleSchedule>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ModuleSchedule>().HasIndex(s => s.ModuleId).IsUnique();

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

        // Prevent duplicate owner assignments for a module and account.
        builder.Entity<ModuleOwnerAssignment>(entity =>
        {
            entity.HasIndex(assignment => new
            {
                assignment.ModuleId,
                assignment.OwnerUserId
            }).IsUnique();

            entity.HasOne(assignment => assignment.Owner)
                .WithMany()
                .HasForeignKey(assignment => assignment.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Each learner can enrol in a given module only once.
        builder.Entity<ModuleEnrollment>(entity =>
        {
            entity.HasIndex(enrollment => new
            {
                enrollment.UserId,
                enrollment.ModuleId
            }).IsUnique();

            entity.HasOne(enrollment => enrollment.User)
                .WithMany()
                .HasForeignKey(enrollment => enrollment.UserId)
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

            // Keep the bank available so completed attempts remain reviewable.
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

            // Preserve the question when an answer is part of attempt history.
            entity.HasOne(answer => answer.Question)
                .WithMany()
                .HasForeignKey(answer => answer.AssessmentQuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}