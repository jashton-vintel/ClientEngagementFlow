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

        public ProcessingJobNotificationsController(IHubContext<ProcessingJobsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> NotifyStatusChanged(ProcessingJobStatusChangedRequest request, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync(
                "JobStatusChanged",
                request.JobId,
                request.Status,
                cancellationToken);

            return Ok();
        }
    }
}