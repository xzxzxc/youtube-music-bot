using System.Threading.Tasks;

namespace YoutubeMusicBot.Application.Abstractions
{
    public interface IInitializable
    {
        int Order => 0;

        ValueTask Initialize();
    }
}
