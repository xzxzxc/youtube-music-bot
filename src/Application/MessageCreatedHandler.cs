using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using YoutubeMusicBot.Application.EventSourcing;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Application
{
    public class MessageCreatedHandler : IEventHandler<MessageCreatedEvent, Message>
    {
        private readonly IValidator<Message> _validator;
        private readonly IRepository<Message> _repository;

        public MessageCreatedHandler(
            IValidator<Message> validator,
            IRepository<Message> repository)
        {
            _validator = validator;
            _repository = repository;
        }

        public async ValueTask Handle(
            MessageCreatedEvent @event,
            CancellationToken cancellationToken = default)
        {
            var message = @event.Aggregate;
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

            await _repository.SaveAndEmitEventsAsync(message, cancellationToken);
        }
    }
}
