using ClientEngagementFlow.Api.Contracts;
using ClientEngagementFlow.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ClientEngagementFlow.Api.Controllers
{
    [ApiController]
    [Route("api/job-notifications")]
    public class ProcessingJobNotificationsController : ControllerBase
    {
        private readonly IHubContext<ProcessingJobsHub> _hubContext;
        private readonly IConfiguration _configuration;

        public ProcessingJobNotificationsController(IHubContext<ProcessingJobsHub> hubContext, IConfiguration configuration)
        {
            _hubContext = hubContext;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> NotifyStatusChanged(ProcessingJobStatusChangedRequest request, CancellationToken cancellationToken)
        {
            var expectedApiKey = _configuration["InternalApi:NotificationApiKey"];
            var suppliedApiKey = Request.Headers["X-Internal-Api-Key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(expectedApiKey) || suppliedApiKey != expectedApiKey)
            {
                return Unauthorized();
            }

            await _hubContext.Clients.All.SendAsync(
                "JobStatusChanged",
                request.JobId,
                request.Status,
                cancellationToken);

            return Ok();
        }
    }
}