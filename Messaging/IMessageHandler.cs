namespace OrderManagement.Messaging;
public interface IMessageHandler<in T>
{
    Task HandleAsync(T message, IDictionary<string, object?>? headers);
}

