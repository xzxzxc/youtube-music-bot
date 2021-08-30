namespace YoutubeMusicBot.Application.Abstractions.Mediator
{
    public interface ICommand
    {
    }

    public interface ICommand<out TResult>
    {
    }
}
