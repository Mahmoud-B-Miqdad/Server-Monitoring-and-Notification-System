namespace MessagingLibrary.Interfaces;

public interface IMessagePublisher
{
    void Publish<T>(string routingKey, T message);
}