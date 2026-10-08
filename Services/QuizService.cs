using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.DTOs;
using QuizApp.Models;

namespace QuizApp.Services;

public interface IQuizService
{
    Task<PagedResult<QuizListDto>> GetQuizzesAsync(QuizFilterDto filter, bool activeOnly = true);
    Task<QuizDetailDto?> GetQuizByIdAsync(int id);
    Task<QuizDetailDto> CreateQuizAsync(CreateQuizDto dto, int createdBy);
    Task<QuizDetailDto> CreateQuizWithQuestionsAsync(CreateQuizWithQuestionsDto dto, int createdBy);
    Task<QuizDetailDto?> UpdateQuizAsync(int id, UpdateQuizDto dto);
    Task<bool> DeleteQuizAsync(int id);
    Task<bool> ToggleQuizStatusAsync(int id);
}

public class QuizService : IQuizService
{
    private readonly AppDbContext _db;

    public QuizService(AppDbContext db) => _db = db;

    public async Task<PagedResult<QuizListDto>> GetQuizzesAsync(QuizFilterDto filter, bool activeOnly = true)
    {
        var query = _db.Quizzes
            .Include(q => q.Category)
            .ThenInclude(c => c.Parent)
            .Include(q => q.Questions)
            .Include(q => q.Attempts)
            .AsQueryable();

        if (activeOnly) query = query.Where(q => q.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(q =>
                q.Title.Contains(filter.Search) ||
                (q.Description != null && q.Description.Contains(filter.Search)));

        if (filter.CategoryId.HasValue)
            query = query.Where(q =>
                q.CategoryId == filter.CategoryId ||
                q.Category.ParentId == filter.CategoryId);

        if (!string.IsNullOrWhiteSpace(filter.Difficulty))
            query = query.Where(q => q.Difficulty == filter.Difficulty);

        if (filter.MinDuration.HasValue)
            query = query.Where(q => q.DurationMinutes >= filter.MinDuration);

        if (filter.MaxDuration.HasValue)
            query = query.Where(q => q.DurationMinutes <= filter.MaxDuration);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(q => new QuizListDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                CategoryName = q.Category.Name,
                CategoryId = q.CategoryId,
                Difficulty = q.Difficulty,
                DurationMinutes = q.DurationMinutes,
                TotalMarks = q.TotalMarks,
                PassingPercentage = q.PassingPercentage,
                QuestionCount = q.Questions.Count(qs => qs.IsActive),
                TotalAttempts = q.Attempts.Count(a => a.Status != "InProgress"),
                IsActive = q.IsActive,
                CreatedAt = q.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<QuizListDto>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<QuizDetailDto?> GetQuizByIdAsync(int id)
    {
        var q = await _db.Quizzes
            .Include(q => q.Category).ThenInclude(c => c.Parent)
            .Include(q => q.Creator)
            .Include(q => q.Questions)
            .Include(q => q.Attempts)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (q == null) return null;

        return new QuizDetailDto
        {
            Id = q.Id,
            Title = q.Title,
            Description = q.Description,
            CategoryName = q.Category.Name,
            CategoryId = q.CategoryId,
            Difficulty = q.Difficulty,
            DurationMinutes = q.DurationMinutes,
            TotalMarks = q.TotalMarks,
            PassingPercentage = q.PassingPercentage,
            NegativeMarking = q.NegativeMarking,
            NegativeMarkValue = q.NegativeMarkValue,
            RandomizeQuestions = q.RandomizeQuestions,
            RandomizeOptions = q.RandomizeOptions,
            QuestionCount = q.Questions.Count(qs => qs.IsActive),
            TotalAttempts = q.Attempts.Count(a => a.Status != "InProgress"),
            IsActive = q.IsActive,
            CreatedByName = q.Creator.Name,
            CreatedAt = q.CreatedAt
        };
    }

    public async Task<QuizDetailDto> CreateQuizAsync(CreateQuizDto dto, int createdBy)
    {
        var quiz = new Quiz
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Difficulty = dto.Difficulty,
            DurationMinutes = dto.DurationMinutes,
            PassingPercentage = dto.PassingPercentage,
            NegativeMarking = dto.NegativeMarking,
            NegativeMarkValue = dto.NegativeMarkValue,
            RandomizeQuestions = dto.RandomizeQuestions,
            RandomizeOptions = dto.RandomizeOptions,
            MaxQuestionsPerAttempt = dto.MaxQuestionsPerAttempt,
            IsActive = dto.IsActive,
            CreatedBy = createdBy
        };

        _db.Quizzes.Add(quiz);
        await _db.SaveChangesAsync();

        return (await GetQuizByIdAsync(quiz.Id))!;
    }

    public async Task<QuizDetailDto> CreateQuizWithQuestionsAsync(CreateQuizWithQuestionsDto dto, int createdBy)
    {
        var quiz = new Quiz
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Difficulty = dto.Difficulty,
            DurationMinutes = dto.DurationMinutes,
            PassingPercentage = dto.PassingPercentage,
            NegativeMarking = dto.NegativeMarking,
            NegativeMarkValue = dto.NegativeMarkValue,
            RandomizeQuestions = dto.RandomizeQuestions,
            RandomizeOptions = dto.RandomizeOptions,
            MaxQuestionsPerAttempt = dto.MaxQuestionsPerAttempt,
            IsActive = dto.IsActive,
            CreatedBy = createdBy
        };

        foreach (var q in dto.Questions)
        {
            quiz.Questions.Add(new Question
            {
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectOption = q.CorrectOption,
                Marks = q.Marks,
                Difficulty = q.Difficulty,
                Explanation = q.Explanation
            });
        }

        quiz.TotalMarks = quiz.Questions.Sum(x => x.Marks);

        // Quiz + all questions are saved in ONE transaction
        _db.Quizzes.Add(quiz);
        await _db.SaveChangesAsync();

        return (await GetQuizByIdAsync(quiz.Id))!;
    }

    public async Task<QuizDetailDto?> UpdateQuizAsync(int id, UpdateQuizDto dto)
    {
        var quiz = await _db.Quizzes.FindAsync(id);
        if (quiz == null) return null;

        quiz.Title = dto.Title;
        quiz.Description = dto.Description;
        quiz.CategoryId = dto.CategoryId;
        quiz.Difficulty = dto.Difficulty;
        quiz.DurationMinutes = dto.DurationMinutes;
        quiz.PassingPercentage = dto.PassingPercentage;
        quiz.NegativeMarking = dto.NegativeMarking;
        quiz.NegativeMarkValue = dto.NegativeMarkValue;
        quiz.RandomizeQuestions = dto.RandomizeQuestions;
        quiz.RandomizeOptions = dto.RandomizeOptions;
        quiz.MaxQuestionsPerAttempt = dto.MaxQuestionsPerAttempt;
        quiz.IsActive = dto.IsActive;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetQuizByIdAsync(id);
    }

    public async Task<bool> DeleteQuizAsync(int id)
    {
        var quiz = await _db.Quizzes.FindAsync(id);
        if (quiz == null) return false;
        _db.Quizzes.Remove(quiz);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleQuizStatusAsync(int id)
    {
        var quiz = await _db.Quizzes.FindAsync(id);
        if (quiz == null) return false;
        quiz.IsActive = !quiz.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }
}
