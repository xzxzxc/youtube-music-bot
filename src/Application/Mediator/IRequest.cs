namespace YoutubeMusicBot.Application.Mediator
{
    public interface IRequest
    {
    }

    public interface IRequest<out TResult>
    {
    }
}
