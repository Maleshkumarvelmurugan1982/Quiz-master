using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.DTOs;
using QuizApp.Models;

namespace QuizApp.Services;

public interface IAttemptService
{
    Task<StartAttemptResponseDto?> StartAttemptAsync(int userId, int quizId);
    Task<AttemptResultDto?> SubmitAttemptAsync(int userId, SubmitAttemptDto dto);
    Task<AttemptResultDto?> GetAttemptResultAsync(int userId, int attemptId);
    Task<List<AttemptHistoryDto>> GetUserHistoryAsync(int userId);
    Task<UserDashboardDto> GetUserDashboardAsync(int userId);
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int currentUserId);
}

public class AttemptService : IAttemptService
{
    private readonly AppDbContext _db;

    public AttemptService(AppDbContext db) => _db = db;

    // --------------------------------------------------------
    // START QUIZ
    // --------------------------------------------------------
    public async Task<StartAttemptResponseDto?> StartAttemptAsync(int userId, int quizId)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Questions.Where(qs => qs.IsActive))
            .FirstOrDefaultAsync(q => q.Id == quizId && q.IsActive);

        if (quiz == null) return null;

        // Cancel any in-progress attempts for this quiz
        var staleAttempts = await _db.Attempts
            .Where(a => a.UserId == userId && a.QuizId == quizId && a.Status == "InProgress")
            .ToListAsync();
        _db.Attempts.RemoveRange(staleAttempts);

        // Create new attempt
        var attempt = new Attempt
        {
            UserId = userId,
            QuizId = quizId,
            TotalMarks = quiz.TotalMarks,
            Status = "InProgress"
        };
        _db.Attempts.Add(attempt);
        await _db.SaveChangesAsync();

        // Get questions (optionally randomized, limited)
        var questions = quiz.Questions.ToList();

        if (quiz.RandomizeQuestions)
            questions = questions.OrderBy(_ => Guid.NewGuid()).ToList();

        if (quiz.MaxQuestionsPerAttempt.HasValue && quiz.MaxQuestionsPerAttempt < questions.Count)
            questions = questions.Take(quiz.MaxQuestionsPerAttempt.Value).ToList();

        // Get user's bookmarks for this quiz
        var bookmarkedIdsList = await _db.BookmarkedQuestions
            .Where(b => b.UserId == userId)
            .Select(b => b.QuestionId)
            .ToListAsync();
        var bookmarkedIds = bookmarkedIdsList.ToHashSet();

        var questionDtos = questions.Select(q =>
        {
            var opts = new[] { q.OptionA, q.OptionB, q.OptionC, q.OptionD };
            if (quiz.RandomizeOptions)
                opts = opts.OrderBy(_ => Guid.NewGuid()).ToArray();

            return new QuizQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                OptionA = opts[0],
                OptionB = opts[1],
                OptionC = opts[2],
                OptionD = opts[3],
                Marks = q.Marks,
                Difficulty = q.Difficulty,
                IsBookmarked = bookmarkedIds.Contains(q.Id)
            };
        }).ToList();

        return new StartAttemptResponseDto
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            DurationSeconds = quiz.DurationMinutes * 60,
            TotalMarks = quiz.TotalMarks,
            PassingPercentage = quiz.PassingPercentage,
            NegativeMarking = quiz.NegativeMarking,
            Questions = questionDtos
        };
    }

    // --------------------------------------------------------
    // SUBMIT QUIZ
    // --------------------------------------------------------
    public async Task<AttemptResultDto?> SubmitAttemptAsync(int userId, SubmitAttemptDto dto)
    {
        var attempt = await _db.Attempts
            .Include(a => a.Quiz)
            .FirstOrDefaultAsync(a => a.Id == dto.AttemptId && a.UserId == userId && a.Status == "InProgress");

        if (attempt == null) return null;

        // Get all question IDs submitted
        var questionIds = dto.Answers.Select(a => a.QuestionId).ToList();
        var questions = await _db.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        decimal totalScore = 0;
        int correct = 0, wrong = 0, unanswered = 0;
        var answerRecords = new List<AttemptAnswer>();

        foreach (var submitted in dto.Answers)
        {
            if (!questions.TryGetValue(submitted.QuestionId, out var question)) continue;

            bool isCorrect = false;
            decimal marksAwarded = 0;

            if (string.IsNullOrEmpty(submitted.SelectedOption))
            {
                unanswered++;
            }
            else if (submitted.SelectedOption == question.CorrectOption)
            {
                isCorrect = true;
                marksAwarded = question.Marks;
                correct++;
                totalScore += marksAwarded;
            }
            else
            {
                wrong++;
                if (attempt.Quiz.NegativeMarking)
                {
                    marksAwarded = -attempt.Quiz.NegativeMarkValue;
                    totalScore += marksAwarded;
                }
            }

            answerRecords.Add(new AttemptAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = submitted.QuestionId,
                SelectedOption = submitted.SelectedOption,
                IsCorrect = isCorrect,
                MarksAwarded = marksAwarded
            });
        }

        if (totalScore < 0) totalScore = 0;

        decimal percentage = attempt.TotalMarks > 0
            ? Math.Round(totalScore / attempt.TotalMarks * 100, 2)
            : 0;

        string status = percentage >= attempt.Quiz.PassingPercentage ? "Passed" : "Failed";

        attempt.Score = totalScore;
        attempt.Percentage = percentage;
        attempt.CorrectAnswers = correct;
        attempt.WrongAnswers = wrong;
        attempt.UnansweredQuestions = unanswered;
        attempt.TimeTakenSeconds = dto.TimeTakenSeconds;
        attempt.Status = status;
        attempt.CompletedAt = DateTime.UtcNow;

        _db.AttemptAnswers.AddRange(answerRecords);
        await _db.SaveChangesAsync();

        return await GetAttemptResultAsync(userId, attempt.Id);
    }

    // --------------------------------------------------------
    // GET RESULT
    // --------------------------------------------------------
    public async Task<AttemptResultDto?> GetAttemptResultAsync(int userId, int attemptId)
    {
        var attempt = await _db.Attempts
            .Include(a => a.Quiz)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

        if (attempt == null || attempt.Status == "InProgress") return null;

        return new AttemptResultDto
        {
            AttemptId = attempt.Id,
            QuizTitle = attempt.Quiz.Title,
            Score = attempt.Score,
            TotalMarks = attempt.TotalMarks,
            Percentage = attempt.Percentage,
            CorrectAnswers = attempt.CorrectAnswers,
            WrongAnswers = attempt.WrongAnswers,
            UnansweredQuestions = attempt.UnansweredQuestions,
            TimeTakenSeconds = attempt.TimeTakenSeconds,
            Status = attempt.Status,
            PassingPercentage = attempt.Quiz.PassingPercentage,
            AttemptedAt = attempt.AttemptedAt,
            ReviewAnswers = attempt.Answers.Select(a => new AnswerReviewDto
            {
                QuestionId = a.QuestionId,
                QuestionText = a.Question.QuestionText,
                OptionA = a.Question.OptionA,
                OptionB = a.Question.OptionB,
                OptionC = a.Question.OptionC,
                OptionD = a.Question.OptionD,
                SelectedOption = a.SelectedOption,
                CorrectOption = a.Question.CorrectOption,
                IsCorrect = a.IsCorrect,
                MarksAwarded = a.MarksAwarded,
                Explanation = a.Question.Explanation
            }).ToList()
        };
    }

    // --------------------------------------------------------
    // HISTORY
    // --------------------------------------------------------
    public async Task<List<AttemptHistoryDto>> GetUserHistoryAsync(int userId)
    {
        return await _db.Attempts
            .Where(a => a.UserId == userId && a.Status != "InProgress")
            .Include(a => a.Quiz).ThenInclude(q => q.Category)
            .OrderByDescending(a => a.AttemptedAt)
            .Select(a => new AttemptHistoryDto
            {
                AttemptId = a.Id,
                QuizTitle = a.Quiz.Title,
                CategoryName = a.Quiz.Category.Name,
                Difficulty = a.Quiz.Difficulty,
                Score = a.Score,
                TotalMarks = a.TotalMarks,
                Percentage = a.Percentage,
                Status = a.Status,
                TimeTakenSeconds = a.TimeTakenSeconds,
                AttemptedAt = a.AttemptedAt
            })
            .ToListAsync();
    }

    // --------------------------------------------------------
    // USER DASHBOARD
    // --------------------------------------------------------
    public async Task<UserDashboardDto> GetUserDashboardAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);

        var attempts = await _db.Attempts
            .Where(a => a.UserId == userId && a.Status != "InProgress")
            .Include(a => a.Quiz).ThenInclude(q => q.Category)
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync();

        var availableQuizzes = await _db.Quizzes.CountAsync(q => q.IsActive);

        var recent = attempts.Take(5).Select(a => new AttemptHistoryDto
        {
            AttemptId = a.Id,
            QuizTitle = a.Quiz.Title,
            CategoryName = a.Quiz.Category.Name,
            Difficulty = a.Quiz.Difficulty,
            Score = a.Score,
            TotalMarks = a.TotalMarks,
            Percentage = a.Percentage,
            Status = a.Status,
            TimeTakenSeconds = a.TimeTakenSeconds,
            AttemptedAt = a.AttemptedAt
        }).ToList();

        var performance = attempts.Take(10).Select(a => new PerformanceDataDto
        {
            QuizTitle = a.Quiz.Title,
            Percentage = a.Percentage,
            AttemptedAt = a.AttemptedAt
        }).OrderBy(p => p.AttemptedAt).ToList();

        return new UserDashboardDto
        {
            WelcomeName = user?.Name ?? "",
            AvailableQuizzes = availableQuizzes,
            QuizzesAttempted = attempts.Count,
            AverageScore = attempts.Count > 0
                ? Math.Round(attempts.Average(a => a.Percentage), 2) : 0,
            HighestScore = attempts.Count > 0
                ? attempts.Max(a => a.Percentage) : 0,
            RecentAttempts = recent,
            PerformanceChart = performance
        };
    }

    // --------------------------------------------------------
    // ADMIN DASHBOARD
    // --------------------------------------------------------
    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var totalUsers = await _db.Users.CountAsync(u => u.Role == "User");
        var totalQuizzes = await _db.Quizzes.CountAsync();
        var totalQuestions = await _db.Questions.CountAsync();
        var totalAttempts = await _db.Attempts.CountAsync(a => a.Status != "InProgress");

        var quizStats = await _db.Quizzes
            .Where(q => q.Attempts.Any())
            .Select(q => new QuizStatsDto
            {
                QuizTitle = q.Title,
                TotalAttempts = q.Attempts.Count(a => a.Status != "InProgress"),
                AverageScore = (decimal)q.Attempts.Where(a => a.Status != "InProgress")
                    .Average(a => (double)a.Percentage),
                PassRate = q.Attempts.Where(a => a.Status != "InProgress").Count() > 0
                    ? (decimal)q.Attempts.Count(a => a.Status == "Passed") * 100 /
                      q.Attempts.Count(a => a.Status != "InProgress") : 0
            })
            .OrderByDescending(s => s.TotalAttempts)
            .Take(10)
            .ToListAsync();

        var recentAttempts = await _db.Attempts
            .Where(a => a.Status != "InProgress")
            .Include(a => a.User)
            .Include(a => a.Quiz)
            .OrderByDescending(a => a.AttemptedAt)
            .Take(20)
            .Select(a => new RecentAttemptAdminDto
            {
                UserName = a.User.Name,
                QuizTitle = a.Quiz.Title,
                Percentage = a.Percentage,
                Status = a.Status,
                AttemptedAt = a.AttemptedAt
            })
            .ToListAsync();

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalQuizzes = totalQuizzes,
            TotalQuestions = totalQuestions,
            TotalAttempts = totalAttempts,
            QuizStats = quizStats,
            RecentAttempts = recentAttempts
        };
    }

    // --------------------------------------------------------
    // LEADERBOARD
    // --------------------------------------------------------
    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(int currentUserId)
    {
        var grouped = await _db.Attempts
            .Where(a => a.Status != "InProgress")
            .Include(a => a.User)
            .GroupBy(a => new { a.UserId, a.User.Name })
            .Select(g => new
            {
                UserId = g.Key.UserId,
                Name = g.Key.Name,
                AverageScore = g.Average(a => (double)a.Percentage),
                HighestScore = g.Max(a => a.Percentage),
                TotalAttempts = g.Count(),
                PassedCount = g.Count(a => a.Status == "Passed")
            })
            .OrderByDescending(g => g.AverageScore)
            .ThenByDescending(g => g.TotalAttempts)
            .Take(50)
            .ToListAsync();

        return grouped.Select((g, index) => new LeaderboardEntryDto
        {
            Rank = index + 1,
            UserName = g.Name,
            AverageScore = Math.Round((decimal)g.AverageScore, 2),
            HighestScore = g.HighestScore,
            TotalAttempts = g.TotalAttempts,
            PassedCount = g.PassedCount,
            IsCurrentUser = g.UserId == currentUserId
        }).ToList();
    }
}