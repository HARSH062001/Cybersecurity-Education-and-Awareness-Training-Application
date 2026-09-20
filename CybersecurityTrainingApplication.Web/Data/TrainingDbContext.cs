using CybersecurityTrainingApplication.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CybersecurityTrainingApplication.Web.Data;

public class TrainingDbContext(DbContextOptions<TrainingDbContext> options) : DbContext(options)
{
    public DbSet<TrainingModule> TrainingModules => Set<TrainingModule>();
    public DbSet<QuestionBank> QuestionBanks => Set<QuestionBank>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<ModuleSectionProgress> ModuleSectionProgress => Set<ModuleSectionProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ModuleSectionProgress>()
            .HasIndex(x => new { x.LearnerKey, x.ModuleId, x.SectionKey })
            .IsUnique();
    }
}
