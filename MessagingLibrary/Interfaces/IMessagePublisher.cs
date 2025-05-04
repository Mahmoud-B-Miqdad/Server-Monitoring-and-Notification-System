namespace MessagingLibrary.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(string message, string routingKey);
}
