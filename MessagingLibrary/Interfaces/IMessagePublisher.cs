namespace MessagingLibrary.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(string routingKey, object message);
}
