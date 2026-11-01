
using OrderManagement.Models;

namespace OrderManagement.Messaging;
public class MessageHandler(ILogger<MessageHandler> logger) : IMessageHandler<JobStatusEvent>
{
    public Task HandleAsync(JobStatusEvent message, IDictionary<string, object?>? headers)
    {
        logger.LogInformation(
                "Handling JobStatusEvent - JobId: {JobId}, OrderId: {OrderId}, Status: {Status}, Timestamp: {Timestamp}",
                message.JobId,
                message.OrderId,
                message.Status,
                message.Timestamp
            );

        return Task.CompletedTask;
    }
}
