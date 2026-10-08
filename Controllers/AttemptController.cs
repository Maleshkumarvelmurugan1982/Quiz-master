using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizApp.DTOs;
using QuizApp.Services;

namespace QuizApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttemptController : ControllerBase
{
    private readonly IAttemptService _attemptService;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public AttemptController(IAttemptService attemptService) => _attemptService = attemptService;

    /// <summary>Start a quiz attempt — returns questions and attempt ID</summary>
    [HttpPost("start/{quizId}")]
    public async Task<IActionResult> StartAttempt(int quizId)
    {
        var result = await _attemptService.StartAttemptAsync(UserId, quizId);
        if (result == null)
            return NotFound(new { message = "Quiz not found or inactive." });
        return Ok(result);
    }

    /// <summary>Submit answers and get result</summary>
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitAttempt([FromBody] SubmitAttemptDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _attemptService.SubmitAttemptAsync(UserId, dto);
        if (result == null)
            return BadRequest(new { message = "Invalid attempt or already submitted." });
        return Ok(result);
    }

    /// <summary>Get result of a specific attempt</summary>
    [HttpGet("{attemptId}/result")]
    public async Task<IActionResult> GetResult(int attemptId)
    {
        var result = await _attemptService.GetAttemptResultAsync(UserId, attemptId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Get all quiz attempts for current user</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var history = await _attemptService.GetUserHistoryAsync(UserId);
        return Ok(history);
    }

    /// <summary>Get user dashboard stats</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _attemptService.GetUserDashboardAsync(UserId);
        return Ok(dashboard);
    }

    /// <summary>Get leaderboard</summary>
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        var leaderboard = await _attemptService.GetLeaderboardAsync(UserId);
        return Ok(leaderboard);
    }
}

// ============================================================
// ADMIN CONTROLLER
// ============================================================
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAttemptService _attemptService;

    public AdminController(IAttemptService attemptService) => _attemptService = attemptService;

    /// <summary>[Admin] Get admin dashboard statistics</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _attemptService.GetAdminDashboardAsync();
        return Ok(dashboard);
    }
}
