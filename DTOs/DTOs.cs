using System.ComponentModel.DataAnnotations;

namespace QuizApp.DTOs;

// ============================================================
// AUTH DTOs
// ============================================================
public class RegisterDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class AuthResponseDto
{
    public string Token { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public int UserId { get; set; }
}

// ============================================================
// QUIZ DTOs
// ============================================================
public class QuizListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string CategoryName { get; set; } = "";
    public int CategoryId { get; set; }
    public string Difficulty { get; set; } = "";
    public int DurationMinutes { get; set; }
    public int TotalMarks { get; set; }
    public decimal PassingPercentage { get; set; }
    public int QuestionCount { get; set; }
    public int TotalAttempts { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class QuizDetailDto : QuizListDto
{
    public bool NegativeMarking { get; set; }
    public decimal NegativeMarkValue { get; set; }
    public bool RandomizeQuestions { get; set; }
    public bool RandomizeOptions { get; set; }
    public string CreatedByName { get; set; } = "";
}

public class CreateQuizDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public string Difficulty { get; set; } = "Medium";

    [Range(1, 180)]
    public int DurationMinutes { get; set; } = 15;

    [Range(0, 100)]
    public decimal PassingPercentage { get; set; } = 50.0m;

    public bool NegativeMarking { get; set; } = false;

    [Range(0, 10)]
    public decimal NegativeMarkValue { get; set; } = 0.25m;

    public bool RandomizeQuestions { get; set; } = false;
    public bool RandomizeOptions { get; set; } = false;
    public int? MaxQuestionsPerAttempt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateQuizDto : CreateQuizDto { }

// ============================================================
// QUESTION DTOs
// ============================================================
public class QuestionDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string QuestionText { get; set; } = "";
    public string OptionA { get; set; } = "";
    public string OptionB { get; set; } = "";
    public string OptionC { get; set; } = "";
    public string OptionD { get; set; } = "";
    public string CorrectOption { get; set; } = "";
    public int Marks { get; set; }
    public string Difficulty { get; set; } = "";
    public string? Explanation { get; set; }
    public bool IsActive { get; set; }
}

// Question for taking quiz — NO correct answer exposed
public class QuizQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = "";
    public string OptionA { get; set; } = "";
    public string OptionB { get; set; } = "";
    public string OptionC { get; set; } = "";
    public string OptionD { get; set; } = "";
    public int Marks { get; set; }
    public string Difficulty { get; set; } = "";
    public bool IsBookmarked { get; set; }
}

public class CreateQuestionDto
{
    [Required]
    public int QuizId { get; set; }

    [Required]
    public string QuestionText { get; set; } = "";

    [Required, MaxLength(500)]
    public string OptionA { get; set; } = "";

    [Required, MaxLength(500)]
    public string OptionB { get; set; } = "";

    [Required, MaxLength(500)]
    public string OptionC { get; set; } = "";

    [Required, MaxLength(500)]
    public string OptionD { get; set; } = "";

    [Required, RegularExpression("^[ABCD]$")]
    public string CorrectOption { get; set; } = "A";

    [Range(1, 10)]
    public int Marks { get; set; } = 1;

    public string Difficulty { get; set; } = "Medium";

    public string? Explanation { get; set; }
}

public class UpdateQuestionDto : CreateQuestionDto { }

// ============================================================
// ATTEMPT DTOs
// ============================================================
public class StartAttemptDto
{
    public int QuizId { get; set; }
}

public class StartAttemptResponseDto
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = "";
    public int DurationSeconds { get; set; }
    public int TotalMarks { get; set; }
    public decimal PassingPercentage { get; set; }
    public bool NegativeMarking { get; set; }
    public List<QuizQuestionDto> Questions { get; set; } = [];
}

public class SubmitAnswerDto
{
    public int QuestionId { get; set; }
    public string? SelectedOption { get; set; } // null = unanswered
}

public class SubmitAttemptDto
{
    public int AttemptId { get; set; }
    public int TimeTakenSeconds { get; set; }
    public List<SubmitAnswerDto> Answers { get; set; } = [];
}

public class AttemptResultDto
{
    public int AttemptId { get; set; }
    public string QuizTitle { get; set; } = "";
    public decimal Score { get; set; }
    public int TotalMarks { get; set; }
    public decimal Percentage { get; set; }
    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }
    public int UnansweredQuestions { get; set; }
    public int TimeTakenSeconds { get; set; }
    public string Status { get; set; } = "";
    public decimal PassingPercentage { get; set; }
    public DateTime AttemptedAt { get; set; }
    public List<AnswerReviewDto> ReviewAnswers { get; set; } = [];
}

public class AnswerReviewDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = "";
    public string OptionA { get; set; } = "";
    public string OptionB { get; set; } = "";
    public string OptionC { get; set; } = "";
    public string OptionD { get; set; } = "";
    public string? SelectedOption { get; set; }
    public string CorrectOption { get; set; } = "";
    public bool IsCorrect { get; set; }
    public decimal MarksAwarded { get; set; }
    public string? Explanation { get; set; }
}

public class AttemptHistoryDto
{
    public int AttemptId { get; set; }
    public string QuizTitle { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public decimal Score { get; set; }
    public int TotalMarks { get; set; }
    public decimal Percentage { get; set; }
    public string Status { get; set; } = "";
    public int TimeTakenSeconds { get; set; }
    public DateTime AttemptedAt { get; set; }
}

// ============================================================
// DASHBOARD DTOs
// ============================================================
public class UserDashboardDto
{
    public string WelcomeName { get; set; } = "";
    public int AvailableQuizzes { get; set; }
    public int QuizzesAttempted { get; set; }
    public decimal AverageScore { get; set; }
    public decimal HighestScore { get; set; }
    public List<AttemptHistoryDto> RecentAttempts { get; set; } = [];
    public List<PerformanceDataDto> PerformanceChart { get; set; } = [];
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalQuizzes { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalAttempts { get; set; }
    public List<QuizStatsDto> QuizStats { get; set; } = [];
    public List<RecentAttemptAdminDto> RecentAttempts { get; set; } = [];
}

public class QuizStatsDto
{
    public string QuizTitle { get; set; } = "";
    public int TotalAttempts { get; set; }
    public decimal AverageScore { get; set; }
    public decimal PassRate { get; set; }
}

public class PerformanceDataDto
{
    public string QuizTitle { get; set; } = "";
    public decimal Percentage { get; set; }
    public DateTime AttemptedAt { get; set; }
}

public class RecentAttemptAdminDto
{
    public string UserName { get; set; } = "";
    public string QuizTitle { get; set; } = "";
    public decimal Percentage { get; set; }
    public string Status { get; set; } = "";
    public DateTime AttemptedAt { get; set; }
}

// ============================================================
// LEADERBOARD DTOs
// ============================================================
public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string UserName { get; set; } = "";
    public decimal AverageScore { get; set; }
    public decimal HighestScore { get; set; }
    public int TotalAttempts { get; set; }
    public int PassedCount { get; set; }
    public bool IsCurrentUser { get; set; }
}

// ============================================================
// USER MANAGEMENT DTOs (Admin)
// ============================================================
public class UserListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public int TotalAttempts { get; set; }
    public decimal AverageScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}

// ============================================================
// CATEGORY DTOs
// ============================================================
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int QuizCount { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = [];
}

public class CreateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
    public int? ParentId { get; set; }
    public string? Description { get; set; }
}

// ============================================================
// FILTER DTOs
// ============================================================
public class QuizFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? Difficulty { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
