using OrderManagement.Models;

namespace OrderManagement.Messaging.Handlers
{
    public class JobStatusEventHandler(ILogger<JobStatusEventHandler> logger) : IMessageHandler<JobStatusEvent>
    {
        public async Task HandleAsync(JobStatusEvent message, IDictionary<string, object?>? headers)
        {
            logger.LogInformation("Handling JobStatusEvent - JobId: {JobId}", message.JobId);
            await Task.CompletedTask;
        }
    }
}
