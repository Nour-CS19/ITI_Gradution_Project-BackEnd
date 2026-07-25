using Femora.Application.Common.Exceptions;
using Femora.Application.Features.Approvals.Commands.ApplyInstructor;
using Femora.Application.Features.Approvals.Commands.ApplySeller;
using Femora.Application.Features.Approvals.Commands.ReviewApproval;
using Femora.Application.Features.Approvals.Common.Requests;
using Femora.Application.Features.Approvals.Queries.GetPendingApprovals;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Femora.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprovalsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ApprovalsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("instructors/apply")]
        [Authorize]
        public async Task<IActionResult> ApplyInstructor([FromBody] ApplyInstructorRequest request)
        {
            if (request is null)
                return BadRequest("Request body cannot be null.");

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var command = new ApplyInstructorCommand
            {
                UserId = userId,
                Bio = request.Bio,
                PortfolioUrl = request.PortfolioUrl
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (DuplicateApprovalRequestException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("sellers/apply")]
        [Authorize]
        public async Task<IActionResult> ApplySeller(
            [FromBody] ApplySellerRequest request)
        {
            if (request is null)
                return BadRequest("Request body cannot be null.");

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var command = new ApplySellerCommand
            {
                UserId = userId,
                ShopName = request.ShopName,
                Description = request.Description
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (DuplicateApprovalRequestException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("admin/approvals/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            var result =
                await _mediator.Send(new GetPendingApprovalsQuery());

            return Ok(result);
        }

    


    [HttpPost("admin/approvals/{approvalId}/review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewApproval(Guid approvalId, [FromBody] ReviewApprovalApiRequest request)
        {
            var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
                return Unauthorized();

            var command = new ReviewApprovalCommand
            {
                ApprovalId = approvalId,
                AdminId = adminId,
                IsApproved = request.IsApproved,
                Note = request.Note
            };

            var result = await _mediator.Send(command);
            return Ok(new { success = result });
        }
    } 
}
