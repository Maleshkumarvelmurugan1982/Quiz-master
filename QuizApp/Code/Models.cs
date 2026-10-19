using System;
using System.Collections.Generic;

namespace QuizApp.Code
{
    // ============================================================
    // USERS / AUTH
    // ============================================================
    public class AppUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Section { get; set; } // A / B / C for students
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsAdmin { get { return Role == "Admin"; } }
    }

    public class UserListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Section { get; set; }
        public bool IsActive { get; set; }
        public int TotalAttempts { get; set; }
        public decimal AverageScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    // ============================================================
    // CATEGORIES
    // ============================================================
    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string Description { get; set; }
        public int QuizCount { get; set; }
        public List<CategoryItem> SubCategories { get; set; }

        public CategoryItem()
        {
            SubCategories = new List<CategoryItem>();
        }
    }

    // ============================================================
    // QUIZZES
    // ============================================================
    public class QuizListItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Difficulty { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public decimal PassingPercentage { get; set; }
        public int QuestionCount { get; set; }
        public int TotalAttempts { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class QuizDetail : QuizListItem
    {
        public bool NegativeMarking { get; set; }
        public decimal NegativeMarkValue { get; set; }
        public bool RandomizeQuestions { get; set; }
        public bool RandomizeOptions { get; set; }
        public int? MaxQuestionsPerAttempt { get; set; }
        public string CreatedByName { get; set; }
    }

    /// <summary>Quiz settings entered by the admin (create / edit).</summary>
    public class QuizInput
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string Difficulty { get; set; }
        public int DurationMinutes { get; set; }
        public decimal PassingPercentage { get; set; }
        public bool NegativeMarking { get; set; }
        public decimal NegativeMarkValue { get; set; }
        public bool RandomizeQuestions { get; set; }
        public bool RandomizeOptions { get; set; }
        public int? MaxQuestionsPerAttempt { get; set; }
        public bool IsActive { get; set; }

        public QuizInput()
        {
            Difficulty = "Medium";
            DurationMinutes = 15;
            PassingPercentage = 50m;
            NegativeMarkValue = 0.25m;
            IsActive = true;
        }
    }

    // ============================================================
    // QUESTIONS
    // ============================================================
    public class QuestionItem
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectOption { get; set; }
        public int Marks { get; set; }
        public string Difficulty { get; set; }
        public string Explanation { get; set; }
        public bool IsActive { get; set; }

        public string GetOption(string letter)
        {
            switch (letter)
            {
                case "A": return OptionA;
                case "B": return OptionB;
                case "C": return OptionC;
                case "D": return OptionD;
                default: return "";
            }
        }
    }

    /// <summary>
    /// Editable question used by the question editor control and the AI generator.
    /// Serializable because it is kept in ViewState / Session between postbacks.
    /// </summary>
    [Serializable]
    public class QuestionDraft
    {
        public int Id { get; set; } // 0 = new question
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectOption { get; set; }
        public int Marks { get; set; }
        public string Difficulty { get; set; }
        public string Explanation { get; set; }

        public QuestionDraft()
        {
            QuestionText = OptionA = OptionB = OptionC = OptionD = Explanation = "";
            CorrectOption = "A";
            Marks = 1;
            Difficulty = "Medium";
        }
    }

    // ============================================================
    // ATTEMPTS
    // ============================================================

    /// <summary>
    /// The question order (and option order) served for one attempt.
    /// Stored in Session so grading uses exactly what the user saw.
    /// </summary>
    [Serializable]
    public class AttemptLayout
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public List<int> QuestionIds { get; set; }
        /// <summary>Per question: display order of the original letters, e.g. "CADB".</summary>
        public List<string> OptionOrders { get; set; }

        public AttemptLayout()
        {
            QuestionIds = new List<int>();
            OptionOrders = new List<string>();
        }
    }

    public class AttemptInfo
    {
        public int AttemptId { get; set; }
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public decimal PassingPercentage { get; set; }
        public bool NegativeMarking { get; set; }
        public decimal NegativeMarkValue { get; set; }
        public string Status { get; set; }
        public DateTime AttemptedAt { get; set; }
    }

    /// <summary>One question as shown on the Take Quiz page.</summary>
    public class TakeQuestion
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public int Marks { get; set; }
        public string Difficulty { get; set; }
        public bool IsBookmarked { get; set; }
        public List<TakeOption> Options { get; set; }
    }

    public class TakeOption
    {
        public int QuestionId { get; set; }
        public string DisplayLetter { get; set; }
        public string Value { get; set; } // original letter, used for grading
        public string Text { get; set; }
    }

    public class AttemptHistoryItem
    {
        public int AttemptId { get; set; }
        public string QuizTitle { get; set; }
        public string CategoryName { get; set; }
        public string Difficulty { get; set; }
        public decimal Score { get; set; }
        public int TotalMarks { get; set; }
        public decimal Percentage { get; set; }
        public string Status { get; set; }
        public int TimeTakenSeconds { get; set; }
        public DateTime AttemptedAt { get; set; }
    }

    public class AttemptResult
    {
        public int AttemptId { get; set; }
        public int UserId { get; set; }
        public string QuizTitle { get; set; }
        public decimal Score { get; set; }
        public int TotalMarks { get; set; }
        public decimal Percentage { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int UnansweredQuestions { get; set; }
        public int TimeTakenSeconds { get; set; }
        public string Status { get; set; }
        public decimal PassingPercentage { get; set; }
        public DateTime AttemptedAt { get; set; }
        public List<AnswerReview> Review { get; set; }

        public bool Passed { get { return Status == "Passed"; } }
    }

    public class AnswerReview
    {
        public int Number { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string SelectedOption { get; set; }
        public string CorrectOption { get; set; }
        public bool IsCorrect { get; set; }
        public decimal MarksAwarded { get; set; }
        public string Explanation { get; set; }
    }

    // ============================================================
    // DASHBOARDS / LEADERBOARD
    // ============================================================
    public class UserDashboard
    {
        public string WelcomeName { get; set; }
        public int AvailableQuizzes { get; set; }
        public int QuizzesAttempted { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }
        public List<AttemptHistoryItem> RecentAttempts { get; set; }
        public List<PerformancePoint> Performance { get; set; }
    }

    public class PerformancePoint
    {
        public string QuizTitle { get; set; }
        public decimal Percentage { get; set; }
        public DateTime AttemptedAt { get; set; }
    }

    public class AdminDashboard
    {
        public int TotalUsers { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAttempts { get; set; }
        public List<QuizStat> QuizStats { get; set; }
        public List<RecentAdminAttempt> RecentAttempts { get; set; }
    }

    public class QuizStat
    {
        public string QuizTitle { get; set; }
        public int TotalAttempts { get; set; }
        public decimal AverageScore { get; set; }
        public decimal PassRate { get; set; }
    }

    public class RecentAdminAttempt
    {
        public string UserName { get; set; }
        public string QuizTitle { get; set; }
        public decimal Percentage { get; set; }
        public string Status { get; set; }
        public DateTime AttemptedAt { get; set; }
    }

    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }
        public int TotalAttempts { get; set; }
        public int PassedCount { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    // ============================================================
    // AI GENERATOR
    // ============================================================
    [Serializable]
    public class GeneratedQuiz
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public int SuggestedDurationMinutes { get; set; }
        public List<QuestionDraft> Questions { get; set; }
        public string Warning { get; set; }
        public string ModelUsed { get; set; }
        public int ModelsTried { get; set; }

        public GeneratedQuiz()
        {
            Title = Description = Warning = ModelUsed = "";
            Difficulty = "Medium";
            SuggestedDurationMinutes = 15;
            Questions = new List<QuestionDraft>();
            ModelsTried = 1;
        }
    }
}
