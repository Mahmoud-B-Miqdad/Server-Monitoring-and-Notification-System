namespace MessagingLibrary.Interfaces;

public interface IMessagePublisher
{
    void Publish(string routingKey, object message);
}
