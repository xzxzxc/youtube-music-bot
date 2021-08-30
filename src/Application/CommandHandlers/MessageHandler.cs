using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application.CommandHandlers
{
    public class MessageHandler : ICommandHandler<MessageHandler.Command>
    {
        private readonly IRepository<Message> _messageRepository;

        public MessageHandler(IRepository<Message> messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async ValueTask Handle(
            Command command,
            CancellationToken cancellationToken = default)
        {
            var message = new Message(
                command.MessageId,
                command.Text,
                command.ChatId);
            await _messageRepository.SaveAndEmitEventsAsync(message, cancellationToken);
        }

        public record Command(
            int MessageId,
            long ChatId,
            string Text) : ICommand;
    }
}
