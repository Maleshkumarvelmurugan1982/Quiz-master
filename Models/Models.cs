using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Models;

// ============================================================
// USER
// ============================================================
public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Required, MaxLength(200)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [MaxLength(20)]
    public string Role { get; set; } = "User"; // "User" or "Admin"

    public bool IsActive { get; set; } = true;

    public string? ProfilePicture { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLogin { get; set; }

    // Navigation
    public ICollection<Attempt> Attempts { get; set; } = [];
    public ICollection<BookmarkedQuestion> BookmarkedQuestions { get; set; } = [];
}

// ============================================================
// CATEGORY
// ============================================================
public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    public int? ParentId { get; set; }

    [ForeignKey("ParentId")]
    public Category? Parent { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Category> SubCategories { get; set; } = [];
    public ICollection<Quiz> Quizzes { get; set; } = [];
}

// ============================================================
// QUIZ
// ============================================================
public class Quiz
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    [MaxLength(20)]
    public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard

    public int DurationMinutes { get; set; } = 15;

    public int TotalMarks { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")]
    public decimal PassingPercentage { get; set; } = 50.0m;

    public bool NegativeMarking { get; set; } = false;

    [Column(TypeName = "decimal(5,2)")]
    public decimal NegativeMarkValue { get; set; } = 0.25m;

    public bool RandomizeQuestions { get; set; } = false;

    public bool RandomizeOptions { get; set; } = false;

    public int? MaxQuestionsPerAttempt { get; set; }

    public bool IsActive { get; set; } = true;

    public int CreatedBy { get; set; }
    public User Creator { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Question> Questions { get; set; } = [];
    public ICollection<Attempt> Attempts { get; set; } = [];
}

// ============================================================
// QUESTION
// ============================================================
public class Question
{
    public int Id { get; set; }

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

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

    [Required, MaxLength(1)]
    public string CorrectOption { get; set; } = "A"; // A, B, C, D

    public int Marks { get; set; } = 1;

    [MaxLength(20)]
    public string Difficulty { get; set; } = "Medium";

    public string? Explanation { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AttemptAnswer> Answers { get; set; } = [];
    public ICollection<BookmarkedQuestion> Bookmarks { get; set; } = [];
}

// ============================================================
// ATTEMPT
// ============================================================
public class Attempt
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Score { get; set; } = 0;

    public int TotalMarks { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")]
    public decimal Percentage { get; set; } = 0;

    public int CorrectAnswers { get; set; } = 0;

    public int WrongAnswers { get; set; } = 0;

    public int UnansweredQuestions { get; set; } = 0;

    public int TimeTakenSeconds { get; set; } = 0;

    [MaxLength(20)]
    public string Status { get; set; } = "InProgress"; // InProgress, Passed, Failed

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}

// ============================================================
// ATTEMPT ANSWER
// ============================================================
public class AttemptAnswer
{
    public int Id { get; set; }

    public int AttemptId { get; set; }
    public Attempt Attempt { get; set; } = null!;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public string? SelectedOption { get; set; } // A, B, C, D or null

    public bool IsCorrect { get; set; } = false;

    [Column(TypeName = "decimal(5,2)")]
    public decimal MarksAwarded { get; set; } = 0;
}

// ============================================================
// BOOKMARKED QUESTION
// ============================================================
public class BookmarkedQuestion
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public DateTime BookmarkedAt { get; set; } = DateTime.UtcNow;
}
