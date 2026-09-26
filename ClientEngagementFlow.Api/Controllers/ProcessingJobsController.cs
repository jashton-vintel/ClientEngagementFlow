using ClientEngagementFlow.Api.Contracts;
using ClientEngagementFlow.Application.Abstractions.Persistence;
using ClientEngagementFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace ClientEngagementFlow.Api.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    [Authorize]
    public class ProcessingJobsController : ControllerBase
    {
        private readonly IProcessingJobStore _store;

        public ProcessingJobsController(IProcessingJobStore store)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var jobs = await _store.GetAllAsync(cancellationToken);

            return Ok(jobs.Select(ConvertToDto));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id,CancellationToken cancellationToken)
        {
            var job = await _store.GetByIdAsync(id, cancellationToken);

            if (job is null)
                return NotFound();

            return Ok(ConvertToDto(job));
        }

        [RequiredScope("jobs.submit")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProcessingJobRequest request, CancellationToken cancellationToken)
        {
            if (request.DocumentId == Guid.Empty)
            {
                return BadRequest("DocumentId is required");
            }

            var job = new ProcessingJob(request.DocumentId);

            await _store.AddAsync(job, cancellationToken);

            var response = ConvertToDto(job);

            // Return 202 since jobs are still processing
            return AcceptedAtAction(nameof(GetById), new { id = job.Id }, response);
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                Name = User.Identity?.Name,
                Claims = User.Claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
            });
        }

        [AllowAnonymous]
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                Status = "Healthy"
            });
        }

        private static ProcessingJobResponse ConvertToDto(ProcessingJob job)
        {
            return new ProcessingJobResponse
            {
                Id = job.Id,
                DocumentId = job.DocumentId,
                Status = job.Status,
                CreatedUtc = job.CreatedUtc,
                StartedUtc = job.StartedUtc,
                CompletedUtc = job.CompletedUtc,
                FailureReason = job.FailureReason
            };
        }
    }
}
