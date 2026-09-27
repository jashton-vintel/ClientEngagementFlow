using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ClientEngagementFlow.Api.Hubs
{
    [Authorize]
    public class ProcessingJobsHub : Hub
    {
    }
}