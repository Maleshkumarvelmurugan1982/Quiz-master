using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.DTOs;
using QuizApp.Models;

namespace QuizApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoryController(AppDbContext db) => _db = db;

    /// <summary>Get all categories with subcategories</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .Where(c => c.ParentId == null && c.IsActive)
            .Include(c => c.SubCategories)
            .Include(c => c.Quizzes)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                QuizCount = c.Quizzes.Count(q => q.IsActive),
                SubCategories = c.SubCategories.Where(s => s.IsActive).Select(s => new CategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ParentId = s.ParentId,
                    ParentName = c.Name,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    QuizCount = s.Quizzes.Count(q => q.IsActive)
                }).ToList()
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>[Admin] Create category</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var category = new Category
        {
            Name = dto.Name,
            ParentId = dto.ParentId,
            Description = dto.Description
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return Ok(new { category.Id, category.Name, category.ParentId });
    }

    /// <summary>[Admin] Delete category</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return NotFound();

        var hasQuizzes = await _db.Quizzes.AnyAsync(q => q.CategoryId == id);
        if (hasQuizzes)
            return BadRequest(new { message = "Cannot delete category with associated quizzes." });

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ============================================================
// USER MANAGEMENT CONTROLLER (Admin)
// ============================================================
[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public UserController(AppDbContext db) => _db = db;

    /// <summary>[Admin] Get all users</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Users
            .Where(u => u.Role == "User")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));

        var total = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                TotalAttempts = u.Attempts.Count(a => a.Status != "InProgress"),
                AverageScore = u.Attempts.Any(a => a.Status != "InProgress")
                    ? (decimal)Math.Round(u.Attempts.Where(a => a.Status != "InProgress")
                        .Average(a => (double)a.Percentage), 2) : 0,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin
            })
            .ToListAsync();

        return Ok(new PagedResult<UserListDto>
        {
            Items = users,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>[Admin] Get user's attempt history</summary>
    [HttpGet("{id}/attempts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserAttempts(int id)
    {
        var attempts = await _db.Attempts
            .Where(a => a.UserId == id && a.Status != "InProgress")
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

        return Ok(attempts);
    }

    /// <summary>[Admin] Toggle user active/inactive</summary>
    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (id == CurrentUserId)
            return BadRequest(new { message = "Cannot disable your own account." });

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        return Ok(new { user.IsActive, message = $"User {(user.IsActive ? "activated" : "deactivated")}." });
    }

    /// <summary>Get current user profile</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.LastLogin
        });
    }
}