using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverFlowClone.Models.DTOs.Question;
using StackOverFlowClone.Services.Interfaces;
using System.Security.Claims;
//"test"
[ApiController]
[Route("api/[controller]")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [Authorize(Policy = "CanAskQuestion")]
    [HttpPost]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto dto)
    {
        if (dto == null)
            return BadRequest("Question data is required.");

        var userid = GetUserIdFromClaims();
        var result = await _questionService.CreateQuestionAsync(dto, userid);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetQuestions(int pageNumber, int size)
    {
        if (pageNumber <= 0 || size <= 0)
            return BadRequest("Page number and size must be greater than zero.");

        var result = await _questionService.GetAllQuestionsAsync(pageNumber, size);
        return Ok(result);
    }

    [HttpGet("Question/{id}")]
    public async Task<IActionResult> GetQuestion(int id)
    {
        var result = await _questionService.GetQuestionByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("QuestionByNewset")]
    public async Task<IActionResult> GetQuestionsByNewsetDate(int pageNumber, int size)
    {
        if (pageNumber <= 0 || size <= 0)
            return BadRequest("Page number and size must be greater than zero.");

        var result = await _questionService.GetQuestionsByNewsetDate(pageNumber, size);
        return Ok(result);
    }

    [HttpGet("QuestionByMostVoted")]
    public async Task<IActionResult> GetQuestionsByMostVoted(int pageNumber, int size)
    {
        if (pageNumber <= 0 || size <= 0)
            return BadRequest("Page number and size must be greater than zero.");

        var result = await _questionService.GetQuestionsByMostVoted(pageNumber, size);
        return Ok(result);
    }

    [HttpGet("uestionsByTag")]
    public async Task<IActionResult> GetQuestionsByTag(string tag, int pageNumber, int size)
    {
        if (string.IsNullOrEmpty(tag))
            return BadRequest("tag shouldn't be empty or null.");

        var result = await _questionService.GetQuestionsByTag(tag, pageNumber, size);
        return Ok(result);
    }

    [HttpGet("QuestionsByPapular")]
    public async Task<IActionResult> GetQuestionsByPapular(int pageNumber, int size)
    {
        if (pageNumber <= 0 || size <= 0)
            return BadRequest("Page number and size must be greater than zero.");

        var result = await _questionService.GetQuestionsByPapular(pageNumber, size);
        return Ok(result);
    }

    [HttpGet("QuestionsByDateRange")]
    public async Task<IActionResult> GetQuestionsByDateRange(DateTime start, DateTime end, int pageNumber, int size)
    {
        if (pageNumber <= 0 || size <= 0)
            return BadRequest("Page number and size must be greater than zero.");

        var result = await _questionService.GetQuestionsByDateRange(start, end, pageNumber, size);
        return Ok(result);
    }

    [HttpGet("QuestionsWithVotesMoreThan")]
    public async Task<IActionResult> GetQuestionsWithVotesMoreThan(int numberOfVotes, int pageNumber, int size)
    {
        if (pageNumber <= 0 || size <= 0)
            return BadRequest("Page number and size must be greater than zero.");

        var result = await _questionService.GetQuestionsWithVotesMoreThan(numberOfVotes, pageNumber, size);
        return Ok(result);
    }

    [Authorize(Policy = "CanEditOwnPost")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionDto dto)
    {
        var userId = GetUserIdFromClaims();
        var success = await _questionService.UpdateQuestionAsync(id, dto,userId);
        return success ? Ok("Updated") : NotFound();
    }

    [Authorize(Policy = "CanDeleteOwnPost")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        int userid = GetUserIdFromClaims();
        var success = await _questionService.DeleteQuestionAsync(id, userid);
        return success ? Ok("Deleted") : NotFound();
    }

    [Authorize(Policy = "CanVote")]
    [HttpPost("{id}/vote")]
    public async Task<IActionResult> VoteQuestion(int id, [FromQuery] bool isUpvote)
    {
        int userid = GetUserIdFromClaims();
        var success = await _questionService.VoteQuestionAsync(id, userid, isUpvote);
        return success ? Ok("Voted") : NotFound();
    }

    [HttpGet("{id}/votes")]
    public async Task<IActionResult> GetQuestionVoteCount(int id)
    {
        var count = await _questionService.GetQuestionVoteCountAsync(id);
        return Ok(count);
    }

    [HttpGet("{id}/Count")]
    public async Task<IActionResult> GetQuestionVoteCountAsync(int id)
    {
        var questions = await _questionService.GetQuestionVoteCountAsync(id);
        return Ok(questions);
    }

    [Authorize(Policy = "CanDeleteAnyPost")]
    [HttpDelete("deleteAll")]
    public async Task<IActionResult> DeleteAllQuestions()
    {
        var success = await _questionService.DeleteAllQuestions();
        return success ? Ok("All questions deleted") : BadRequest("Failed to delete all questions");
    }

    [HttpGet("{tag}")]
    public async Task<IActionResult> GetAllQuestions(string tag)
    {
        var questions = await _questionService.GetQuestionsWithTagAsync(tag);
        return Ok(questions);
    }

    [HttpGet("questions/count/{tagname}")]
    public async Task<IActionResult> GetQuestionsCountForTagAsync(string tagname)
    {
        var questions = await _questionService.GetQuestionsWithTagAsync(tagname);
        return Ok(questions);
    }

    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("User is not authenticated.");

        return int.Parse(userIdClaim.Value);
    }
}
