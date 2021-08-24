using System.Threading.Tasks;

namespace YoutubeMusicBot.Application.Interfaces
{
    public interface IInitializable
    {
        int Order => 0;

        ValueTask Initialize();
    }
}
