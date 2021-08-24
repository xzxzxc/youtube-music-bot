using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Domain
{
    public class Message : AggregateBase<Message>
    {
        private Message()
        {
        }

        public Message(int externalId, string text, long chatId)
        {
            RaiseEvent(new MessageCreatedEvent(externalId, text, chatId));
        }

        public int ExternalId { get; private set; }

        public string Text { get; private set; } = string.Empty;

        public long ChatId { get; private set; }

        public bool? IsValid { get; private set; }

        public void Valid()
        {
            RaiseEvent(new MessageValidEvent());
        }

        public void Invalid(string validationMessage)
        {
            RaiseEvent(new MessageInvalidEvent(validationMessage));
        }

        public void Apply(MessageCreatedEvent @event)
        {
            Id = @event.AggregateId;
            ExternalId = @event.ExternalId;
            Text = @event.Text;
            ChatId = @event.ChatId;
        }

        public void Apply(MessageValidEvent @event)
        {
            IsValid = true;
        }

        public void Apply(MessageInvalidEvent @event)
        {
            IsValid = false;
        }
    }
}
