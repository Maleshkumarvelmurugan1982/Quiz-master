using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizApp.DTOs;
using QuizApp.Services;

namespace QuizApp.Controllers;

// ============================================================
// QUIZ CONTROLLER
// ============================================================
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IConfiguration _configuration;

    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin =>
        User.IsInRole("Admin");

    public QuizController(
        IQuizService quizService,
        IConfiguration configuration)
    {
        _quizService = quizService;
        _configuration = configuration;
    }

    /// <summary>
    /// Get paginated list of quizzes with filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetQuizzes(
        [FromQuery] QuizFilterDto filter)
    {
        var result =
            await _quizService.GetQuizzesAsync(
                filter,
                activeOnly: !IsAdmin);

        return Ok(result);
    }

    /// <summary>
    /// Get quiz detail by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuiz(int id)
    {
        var quiz =
            await _quizService.GetQuizByIdAsync(id);

        if (quiz == null)
            return NotFound();

        return Ok(quiz);
    }

    /// <summary>
    /// [Admin] Create a new quiz
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuiz(
        [FromBody] CreateQuizDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var quiz =
            await _quizService.CreateQuizAsync(
                dto,
                UserId);

        return CreatedAtAction(
            nameof(GetQuiz),
            new { id = quiz.Id },
            quiz);
    }

    /// <summary>
    /// [Admin] Generate a quiz DRAFT with AI
    /// from a topic and/or uploaded file.
    /// Nothing is saved to the database.
    /// </summary>
    [HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(15_000_000)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 15_000_000)]
    public async Task<IActionResult> Generate(
        [FromForm] GenerateQuizRequest request,
        [FromServices] IQuizGeneratorService generator,
        CancellationToken ct)
    {
        try
        {
            var draft =
                await generator.GenerateAsync(
                    request,
                    ct);

            return Ok(draft);
        }
        catch (QuizGenerationException ex)
        {
            return StatusCode(
                ex.StatusCode,
                new
                {
                    message = ex.Message
                });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new
                {
                    message =
                        "An unexpected error occurred while generating the quiz.",
                    detail = ex.Message
                });
        }
    }

    /// <summary>
    /// [Admin] Get the currently configured
    /// OpenRouter AI model.
    ///
    /// This replaces the old Gemini model catalog endpoint.
    /// </summary>
    [HttpGet("models")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAiModels()
    {
        var model =
            _configuration["OpenRouter:Model"]
            ?? "openrouter/free";

        return Ok(
            new[]
            {
                new
                {
                    name = model,
                    displayName = model,
                    coolingDown = false
                }
            });
    }

    /// <summary>
    /// [Admin] Save a quiz together with
    /// its questions.
    /// </summary>
    [HttpPost("with-questions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQuizWithQuestions(
        [FromBody] CreateQuizWithQuestionsDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var quiz =
            await _quizService
                .CreateQuizWithQuestionsAsync(dto, UserId);

        return CreatedAtAction(
            nameof(GetQuiz),
            new { id = quiz.Id },
            quiz);
    }

    /// <summary>
    /// [Admin] Update a quiz
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateQuiz(
        int id,
        [FromBody] UpdateQuizDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var quiz =
            await _quizService.UpdateQuizAsync(
                id,
                dto);

        if (quiz == null)
            return NotFound();

        return Ok(quiz);
    }

    /// <summary>
    /// [Admin] Delete a quiz
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        var result =
            await _quizService.DeleteQuizAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// [Admin] Toggle quiz active/inactive status
    /// </summary>
    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleQuiz(int id)
    {
        var result =
            await _quizService
                .ToggleQuizStatusAsync(id);

        if (!result)
            return NotFound();

        return Ok(
            new
            {
                message =
                    "Quiz status toggled."
            });
    }
}


// ============================================================
// QUESTION CONTROLLER
// ============================================================
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;

    private int UserId =>
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

    public QuestionController(
        IQuestionService questionService)
    {
        _questionService =
            questionService;
    }

    /// <summary>
    /// Get all questions for a quiz.
    /// Admin sees correct answers.
    /// </summary>
    [HttpGet("quiz/{quizId}")]
    public async Task<IActionResult> GetByQuiz(
        int quizId)
    {
        var questions =
            await _questionService
                .GetByQuizIdAsync(quizId);

        return Ok(questions);
    }

    /// <summary>
    /// Get question by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id)
    {
        var question =
            await _questionService
                .GetByIdAsync(id);

        if (question == null)
            return NotFound();

        return Ok(question);
    }

    /// <summary>
    /// [Admin] Add a question to a quiz
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuestionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var question =
            await _questionService
                .CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = question.Id },
            question);
    }

    /// <summary>
    /// [Admin] Update a question
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateQuestionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var question =
            await _questionService
                .UpdateAsync(id, dto);

        if (question == null)
            return NotFound();

        return Ok(question);
    }

    /// <summary>
    /// [Admin] Delete a question
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var result =
            await _questionService
                .DeleteAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// [Admin] Toggle question active/inactive
    /// </summary>
    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Toggle(
        int id)
    {
        var result =
            await _questionService
                .ToggleStatusAsync(id);

        if (!result)
            return NotFound();

        return Ok(
            new
            {
                message =
                    "Question status toggled."
            });
    }

    /// <summary>
    /// Bookmark a question
    /// </summary>
    [HttpPost("{id}/bookmark")]
    public async Task<IActionResult> Bookmark(
        int id)
    {
        await _questionService
            .BookmarkAsync(
                UserId,
                id);

        return Ok(
            new
            {
                message =
                    "Question bookmarked."
            });
    }

    /// <summary>
    /// Remove bookmark from a question
    /// </summary>
    [HttpDelete("{id}/bookmark")]
    public async Task<IActionResult> Unbookmark(
        int id)
    {
        await _questionService
            .UnbookmarkAsync(
                UserId,
                id);

        return Ok(
            new
            {
                message =
                    "Bookmark removed."
            });
    }

    /// <summary>
    /// Get all bookmarked questions
    /// for the current user
    /// </summary>
    [HttpGet("bookmarks")]
    public async Task<IActionResult> GetBookmarks()
    {
        var questions =
            await _questionService
                .GetBookmarksAsync(UserId);

        return Ok(questions);
    }
}