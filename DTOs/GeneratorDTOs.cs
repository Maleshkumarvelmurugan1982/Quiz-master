using System.ComponentModel.DataAnnotations;

namespace QuizApp.DTOs;

// ============================================================
// AI QUIZ GENERATION DTOs
// ============================================================

/// <summary>Multipart form sent by the admin: a topic and/or a file.</summary>
public class GenerateQuizRequest
{
    /// <summary>Topic to build the quiz on (required when no file is uploaded).</summary>
    public string? Topic { get; set; }

    /// <summary>Optional extra instructions, e.g. "focus on chapter 3".</summary>
    public string? Instructions { get; set; }

    [Range(3, 30)]
    public int NumQuestions { get; set; } = 10;

    public string Difficulty { get; set; } = "Medium";

    /// <summary>Optional Gemini model to try first (blank = automatic). Other models are still used as fallback.</summary>
    public string? PreferredModel { get; set; }

    /// <summary>PDF, DOCX, PPTX, TXT, MD, CSV, JSON, PNG, JPG, WEBP, GIF (max 10 MB)</summary>
    public IFormFile? File { get; set; }
}

/// <summary>A generated question. Nothing is saved until the admin confirms.</summary>
public class GeneratedQuestionDto
{
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
    public string? Explanation { get; set; }
    public string Difficulty { get; set; } = "Medium";
    [Range(1, 100)]
    public int Marks { get; set; } = 1;
}

/// <summary>Draft returned to the admin for review.</summary>
public class GeneratedQuizDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Difficulty { get; set; } = "Medium";
    public int SuggestedDurationMinutes { get; set; } = 15;
    public List<GeneratedQuestionDto> Questions { get; set; } = [];
    /// <summary>Non-fatal notes, e.g. "AI returned 8 of 10 requested questions".</summary>
    public string? Warning { get; set; }
    /// <summary>Gemini model that produced this draft.</summary>
    public string ModelUsed { get; set; } = "";
    /// <summary>How many models were tried (1 = first one worked).</summary>
    public int ModelsTried { get; set; } = 1;
}

/// <summary>Saves a quiz together with all its questions in one request.</summary>
public class CreateQuizWithQuestionsDto : CreateQuizDto
{
    [Required, MinLength(1)]
    public List<GeneratedQuestionDto> Questions { get; set; } = [];
}
