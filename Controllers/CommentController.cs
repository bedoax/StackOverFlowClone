using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackOverFlowClone.Models.DTOs.Comment;
using StackOverFlowClone.Services.Interfaces;
using System.Security.Claims;

namespace StackOverFlowClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        [Authorize(Roles = "user")]
        [HttpPost("question/{questionId}/comment")]
        public async Task<IActionResult> CreateCommentForQuestionAsync(int questionId, [FromBody] CreateCommentDto commentDto)
        {
            int userId = GetUserIdFromClaims();
            var result = await _commentService.CreateCommentForQuestionAsync(questionId, commentDto, userId);
            return result != null ? Ok(result) : BadRequest("Failed to create comment");
        }
        [Authorize(Roles = "user")]
        [HttpPost("answer/{answerId}/comment")]
        public async Task<IActionResult> CreateCommentForAnswerAsync(int answerId, [FromBody] CreateCommentDto commentDto)
        {
            int userId = GetUserIdFromClaims();
            var result = await _commentService.CreateCommentForAnswerAsync(answerId, commentDto, userId);
            return result != null ? Ok(result) : BadRequest("Failed to create comment");
        }

        [HttpGet("question/{questionId}/comments")]
        public async Task<IActionResult> GetCommentsForQuestionAsync(int questionId)
        {
            var comments = await _commentService.GetCommentForQuestionAsync(questionId);
            return Ok(comments);
        }

        [HttpGet("answer/{answerId}/comments")]
        public async Task<IActionResult> GetCommentForAnswerAsync(int answerId)
        {
            var comments = await _commentService.GetCommentForAnswerAsync(answerId);
            return Ok(comments);
        }
        [Authorize(Roles = "user")]
        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateCommentAsync(int commentId, [FromBody] UpdateCommentDto commentDto)
        {
            var result = await _commentService.UpdateCommentAsync(commentId, commentDto);
            return result ? Ok("Comment updated successfully") : NotFound("Comment not found");
        }
        [Authorize(Roles = "user")]
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteCommentAsync(int commentId)
        {
            int userId = GetUserIdFromClaims();
            var result = await _commentService.DeleteCommentAsync(commentId, userId);
            return result ? Ok("Comment deleted successfully") : NotFound("Comment not found or unauthorized");
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
