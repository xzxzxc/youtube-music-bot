using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class MessageCreatedEventHandler : IEventHandler<MessageCreatedEvent, Message>
    {
        private readonly IValidator<Message> _validator;

        public MessageCreatedEventHandler(
            IValidator<Message> validator)
        {
            _validator = validator;
        }

        public async ValueTask Handle(
            MessageCreatedEvent notification,
            CancellationToken cancellationToken = default)
        {
            var message = notification.Aggregate;
            var validationResult = await _validator.ValidateAsync(
                message,
                cancellationToken);

            if (validationResult.IsValid)
            {
                message.Valid();
            }
            else
            {
                message.Invalid(string.Join('\n', validationResult.Errors));
            }
        }
    }
}
