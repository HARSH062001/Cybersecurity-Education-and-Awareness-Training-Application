using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using CybersecurityTrainingWeb.Utils;

namespace CybersecurityTrainingWeb.Data;

public static class AssessmentSubmissionEndpoints
{
    public static IEndpointRouteBuilder MapAssessmentSubmissionEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/assessments/attempts/{attemptId:long}/submit",
                SubmitAsync)
            .RequireAuthorization();

        endpoints.MapGet(
                "/api/assessments/attempts/{attemptId:long}/answers",
                GetSavedAnswersAsync)
            .RequireAuthorization();

        endpoints.MapPost(
                "/api/assessments/attempts/{attemptId:long}/answers",
                SaveAnswerAsync)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> GetSavedAnswersAsync(
        long attemptId,
        HttpContext http,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        var userId = userManager.GetUserId(http.User);
        if (userId is null)
            return Results.Unauthorized();

        var attemptExists = await db.AssessmentAttempts.AsNoTracking()
            .AnyAsync(a => a.Id == attemptId && a.UserId == userId &&
                           a.Status == AssessmentAttemptStatus.InProgress);
        if (!attemptExists)
            return Results.NotFound();

        http.Response.Headers.CacheControl = "no-store";
        var answers = await db.AssessmentAnswers.AsNoTracking()
            .Where(a => a.AssessmentAttemptId == attemptId)
            .Select(a => new
            {
                questionNumber = a.Question.Number,
                selectedOptionCode = a.SelectedOptionCode
            })
            .ToListAsync();

        return Results.Ok(new { answers });
    }

    private static async Task<IResult> SaveAnswerAsync(
        long attemptId,
        SaveAssessmentAnswerRequest request,
        HttpContext http,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IAntiforgery antiforgery,
        IConfiguration configuration)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(http);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest(new { error = "Invalid request token." });
        }

        var userId = userManager.GetUserId(http.User);
        if (userId is null)
            return Results.Unauthorized();

        var attempt = await db.AssessmentAttempts
            .Include(a => a.Bank)
                .ThenInclude(b => b.Questions)
                    .ThenInclude(q => q.Options)
            .SingleOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

        if (attempt is null)
            return Results.NotFound();
        if (attempt.Status != AssessmentAttemptStatus.InProgress)
            return Results.Conflict(new { error = "This attempt is no longer in progress." });

        if (!await ResultUtils.AssessmentRules.HasCompletedAllSectionsAsync(
                db, userId, attempt.Bank.ModuleId))
        {
            return Results.Conflict(new { error = "Complete all learning sections before saving assessment answers." });
        }

        var passingPercentage = ResultUtils.AssessmentRules.GetPassingPercentage(configuration);
        if (await ResultUtils.HasUserPassed(
                db, attempt.Bank.ModuleId, userId, passingPercentage))
        {
            return Results.Conflict(new { error = "You have already passed this module assessment." });
        }

        var question = attempt.Bank.Questions
            .SingleOrDefault(q => q.Number == request.QuestionNumber);
        var selectedCode = request.SelectedOptionCode?.Trim().ToUpperInvariant();
        if (question is null || question.Type != "multipleChoice" ||
            string.IsNullOrWhiteSpace(selectedCode) ||
            !question.Options.Any(option => option.Code == selectedCode))
        {
            return Results.BadRequest(new { error = "The question or answer option is invalid." });
        }

        var answer = await db.AssessmentAnswers.SingleOrDefaultAsync(a =>
            a.AssessmentAttemptId == attempt.Id &&
            a.AssessmentQuestionId == question.Id);

        if (answer is null)
        {
            answer = new AssessmentAnswer
            {
                AssessmentAttemptId = attempt.Id,
                AssessmentQuestionId = question.Id
            };
            db.AssessmentAnswers.Add(answer);
        }

        answer.SelectedOptionCode = selectedCode;
        answer.IsCorrect = null;
        answer.PointsAwarded = null;
        answer.Feedback = null;
        answer.ReviewedAtUtc = null;
        await db.SaveChangesAsync();

        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(new { saved = true });
    }

    private static async Task<IResult> SubmitAsync(
        long attemptId,
        SubmitAssessmentRequest request,
        HttpContext http,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IAntiforgery antiforgery,
        IConfiguration configuration)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(http);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest(new { error = "Invalid request token." });
        }

        var userId = userManager.GetUserId(http.User);
        if (userId is null)
            return Results.Unauthorized();

        var attempt = await db.AssessmentAttempts
            .Include(a => a.Bank)
                .ThenInclude(b => b.Questions)
                    .ThenInclude(q => q.Options)
            .Include(a => a.Answers)
            .SingleOrDefaultAsync(a =>
                a.Id == attemptId && a.UserId == userId);

        // Do not reveal whether another learner owns this attempt.
        if (attempt is null)
            return Results.NotFound();

        if (attempt.Status != AssessmentAttemptStatus.InProgress)
            return Results.Conflict(new { error = "This attempt was already submitted." });

        if (!await ResultUtils.AssessmentRules.HasCompletedAllSectionsAsync(
                db, userId, attempt.Bank.ModuleId))
        {
            return Results.Conflict(new { error = "Complete all learning sections before submitting." });
        }

        var passingPercentage = ResultUtils.AssessmentRules.GetPassingPercentage(configuration);
        if (await ResultUtils.HasUserPassed(
                db, attempt.Bank.ModuleId, userId, passingPercentage))
        {
            return Results.Conflict(new { error = "You have already passed this module assessment." });
        }

        var questions = attempt.Bank.Questions
            .OrderBy(q => q.Number)
            .ToArray();

        if (questions.Length == 0)
            return Results.BadRequest(new { error = "This bank has no questions." });

        if (questions.Any(q => q.Type != "multipleChoice"))
        {
            return Results.Problem(
                title: "Written answers require a separate review process.",
                statusCode: StatusCodes.Status501NotImplemented);
        }

        // Require exactly one entry per question. A null option means unanswered.
        if (request.Answers is null ||
            request.Answers.Count != questions.Length ||
            request.Answers.Select(a => a.QuestionNumber).Distinct().Count()
                != questions.Length)
        {
            return Results.BadRequest(
                new { error = "Submit one response entry per question." });
        }

        var answers = request.Answers.ToDictionary(a => a.QuestionNumber);
        if (questions.Any(q => !answers.ContainsKey(q.Number)))
        {
            return Results.BadRequest(
                new { error = "The submitted questions do not match this bank." });
        }

        var score = 0;
        var now = DateTime.UtcNow;

        foreach (var question in questions)
        {
            var selectedCode = answers[question.Number]
                .SelectedOptionCode?.Trim().ToUpperInvariant();

            if (selectedCode == "")
                selectedCode = null;

            if (selectedCode is null)
            {
                return Results.BadRequest(
                    new { error = "Answer every question before submitting." });
            }

            if (!question.Options.Any(o => o.Code == selectedCode))
            {
                return Results.BadRequest(
                    new { error = "An answer option is invalid." });
            }

            var isCorrect = selectedCode == question.CorrectOptionCode;

            if (isCorrect)
                score++;

            var savedAnswer = attempt.Answers.SingleOrDefault(
                a => a.AssessmentQuestionId == question.Id);
            if (savedAnswer is null)
            {
                savedAnswer = new AssessmentAnswer
                {
                    AssessmentAttemptId = attempt.Id,
                    AssessmentQuestionId = question.Id
                };
                db.AssessmentAnswers.Add(savedAnswer);
            }

            savedAnswer.SelectedOptionCode = selectedCode;
            savedAnswer.IsCorrect = isCorrect;
            savedAnswer.PointsAwarded = isCorrect ? 1 : 0;
            savedAnswer.Feedback = question.Explanation;
            savedAnswer.ReviewedAtUtc = now;
        }

        attempt.Score = score;
        attempt.TotalQuestions = questions.Length;
        attempt.SubmittedAtUtc = now;
        attempt.MarkedAtUtc = now;
        attempt.Status = AssessmentAttemptStatus.Completed;

        await db.SaveChangesAsync();

        http.Response.Headers.CacheControl = "no-store";
        return Results.Ok(new
        {
            attemptId = attempt.Id,
            score,
            totalQuestions = questions.Length
        });
    }
}

public sealed class SubmitAssessmentRequest
{
    public List<SubmitAssessmentAnswer> Answers { get; set; } = [];
}

public sealed class SubmitAssessmentAnswer
{
    public int QuestionNumber { get; set; }
    public string? SelectedOptionCode { get; set; }
}

public sealed class SaveAssessmentAnswerRequest
{
    public int QuestionNumber { get; set; }
    public string? SelectedOptionCode { get; set; }
}
