using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackOverFlowClone.Models.DTOs.Answer;
using StackOverFlowClone.Models.DTOs.Vote;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Services.Interfaces;
using System.Security.Claims;

namespace StackOverFlowClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly IAnswerService _answerService;

        public AnswerController(IAnswerService answerService)
        {
            _answerService = answerService;
        }
        [HttpGet("{idQuestion}")]
        public async Task<IEnumerable<AnswerDto>> GetAnswerForQuestionAsync(int idQuestion, int pageNumber, int size)
        {
            return await _answerService.GetAnswerForQuestionAsync(idQuestion,pageNumber,size);
           
        }
        [Authorize(Roles = "user")]
        [HttpPost("{idQuestion}")]
        public async Task<IActionResult> CreateAnswerAsync(int idQuestion, [FromBody] CreateAnswerDto answerDto)

        {
            int userId = GetUserIdFromClaims();
            var result = await _answerService.CreateAnswerAsync(idQuestion, answerDto, userId);
            return result != null ? Ok(result) : BadRequest("Failed to create answer");
        }
        [Authorize(Roles = "user")]
        [HttpPut("{idAnswer}")]
        public async Task<IActionResult> UpdateAnswerAsync(int idAnswer, [FromBody] UpdateAnswerDto answerDto)
        {
            var result = await _answerService.UpdateAnswerAsync(idAnswer, answerDto);
            return result ? Ok("Answer updated successfully") : NotFound("Answer not found");
        }
        [Authorize(Roles = "user")]
        [HttpDelete("{idAnswer}")]
        public async Task<IActionResult> DeleteAnswerAsync(int idAnswer)
        {
            int userId = GetUserIdFromClaims();
            var result = await _answerService.DeleteAnswerAsync(idAnswer, userId);
            return result ? Ok("Answer deleted successfully") : NotFound("Answer not found or unauthorized");

        }
        [Authorize(Roles = "user")]
        [HttpPost("vote/{idAnswer}")]
        public async Task<IActionResult> VoteAnswerAsync(int idAnswer, bool isUpove)
        {
            int userId = GetUserIdFromClaims();
            var result = await _answerService.VoteAnswerAsync(idAnswer, userId, isUpove);
            return result ? Ok("Vote recorded successfully") : BadRequest("Failed to record vote");
        }
        [HttpGet("voteCount/{idAnswer}")]
        public async Task<IActionResult> GetAnswerVoteCountAsync(int idAnswer)
        {
            var count = await _answerService.GetAnswerVoteCountAsync(idAnswer);
            return Ok(count);
        }
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            return int.Parse(userIdClaim.Value);
        }
    }
}
