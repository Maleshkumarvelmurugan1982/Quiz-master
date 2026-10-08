using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.DTOs;
using QuizApp.Models;

namespace QuizApp.Services;

// ============================================================
// QUESTION SERVICE
// ============================================================
public interface IQuestionService
{
    Task<List<QuestionDto>> GetByQuizIdAsync(int quizId);
    Task<QuestionDto?> GetByIdAsync(int id);
    Task<QuestionDto> CreateAsync(CreateQuestionDto dto);
    Task<QuestionDto?> UpdateAsync(int id, UpdateQuestionDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ToggleStatusAsync(int id);
    Task<bool> BookmarkAsync(int userId, int questionId);
    Task<bool> UnbookmarkAsync(int userId, int questionId);
    Task<List<QuestionDto>> GetBookmarksAsync(int userId);
}

public class QuestionService : IQuestionService
{
    private readonly AppDbContext _db;

    public QuestionService(AppDbContext db) => _db = db;

    public async Task<List<QuestionDto>> GetByQuizIdAsync(int quizId)
    {
        return await _db.Questions
            .Where(q => q.QuizId == quizId && q.IsActive)
            .Select(q => MapToDto(q))
            .ToListAsync();
    }

    public async Task<QuestionDto?> GetByIdAsync(int id)
    {
        var q = await _db.Questions.FindAsync(id);
        return q == null ? null : MapToDto(q);
    }

    public async Task<QuestionDto> CreateAsync(CreateQuestionDto dto)
    {
        var question = new Question
        {
            QuizId = dto.QuizId,
            QuestionText = dto.QuestionText,
            OptionA = dto.OptionA,
            OptionB = dto.OptionB,
            OptionC = dto.OptionC,
            OptionD = dto.OptionD,
            CorrectOption = dto.CorrectOption,
            Marks = dto.Marks,
            Difficulty = dto.Difficulty,
            Explanation = dto.Explanation
        };

        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        // Recalculate quiz total marks
        await RecalcTotalMarks(dto.QuizId);

        return MapToDto(question);
    }

    public async Task<QuestionDto?> UpdateAsync(int id, UpdateQuestionDto dto)
    {
        var question = await _db.Questions.FindAsync(id);
        if (question == null) return null;

        question.QuestionText = dto.QuestionText;
        question.OptionA = dto.OptionA;
        question.OptionB = dto.OptionB;
        question.OptionC = dto.OptionC;
        question.OptionD = dto.OptionD;
        question.CorrectOption = dto.CorrectOption;
        question.Marks = dto.Marks;
        question.Difficulty = dto.Difficulty;
        question.Explanation = dto.Explanation;

        await _db.SaveChangesAsync();
        await RecalcTotalMarks(question.QuizId);

        return MapToDto(question);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var question = await _db.Questions.FindAsync(id);
        if (question == null) return false;
        var quizId = question.QuizId;
        _db.Questions.Remove(question);
        await _db.SaveChangesAsync();
        await RecalcTotalMarks(quizId);
        return true;
    }

    public async Task<bool> ToggleStatusAsync(int id)
    {
        var question = await _db.Questions.FindAsync(id);
        if (question == null) return false;
        question.IsActive = !question.IsActive;
        await _db.SaveChangesAsync();
        await RecalcTotalMarks(question.QuizId);
        return true;
    }

    public async Task<bool> BookmarkAsync(int userId, int questionId)
    {
        if (await _db.BookmarkedQuestions.AnyAsync(b => b.UserId == userId && b.QuestionId == questionId))
            return false;

        _db.BookmarkedQuestions.Add(new BookmarkedQuestion
        {
            UserId = userId,
            QuestionId = questionId
        });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnbookmarkAsync(int userId, int questionId)
    {
        var bm = await _db.BookmarkedQuestions
            .FirstOrDefaultAsync(b => b.UserId == userId && b.QuestionId == questionId);
        if (bm == null) return false;
        _db.BookmarkedQuestions.Remove(bm);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<QuestionDto>> GetBookmarksAsync(int userId)
    {
        return await _db.BookmarkedQuestions
            .Where(b => b.UserId == userId)
            .Include(b => b.Question)
            .Select(b => MapToDto(b.Question))
            .ToListAsync();
    }

    private async Task RecalcTotalMarks(int quizId)
    {
        var totalMarks = await _db.Questions
            .Where(q => q.QuizId == quizId && q.IsActive)
            .SumAsync(q => q.Marks);

        var quiz = await _db.Quizzes.FindAsync(quizId);
        if (quiz != null)
        {
            quiz.TotalMarks = totalMarks;
            await _db.SaveChangesAsync();
        }
    }

    private static QuestionDto MapToDto(Question q) => new()
    {
        Id = q.Id,
        QuizId = q.QuizId,
        QuestionText = q.QuestionText,
        OptionA = q.OptionA,
        OptionB = q.OptionB,
        OptionC = q.OptionC,
        OptionD = q.OptionD,
        CorrectOption = q.CorrectOption,
        Marks = q.Marks,
        Difficulty = q.Difficulty,
        Explanation = q.Explanation,
        IsActive = q.IsActive
    };
}
