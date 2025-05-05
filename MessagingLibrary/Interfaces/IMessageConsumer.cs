namespace MessagingLibrary.Interfaces;

public interface IMessageConsumer
{
    Task StartConsumingAsync(Func<string, Task> handleMessage);
}