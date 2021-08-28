using System.Collections.Generic;
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

        public int? ProcessMessageId { get; private set; }

        public ICollection<File> Files { get; } = new List<File>();

        public void Valid()
        {
            RaiseEvent(new MessageValidEvent());
        }

        public void Invalid(string validationMessage)
        {
            RaiseEvent(new MessageInvalidEvent(validationMessage));
        }

        public void LoadingProcessMessageSent(int messageId)
        {
            RaiseEvent(new LoadingProcessMessageSentEvent(messageId));
        }

        public void NewMusicFile(string fileFullPath)
        {
            RaiseEvent(new NewMusicFileEvent(fileFullPath));
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

        public void Apply(LoadingProcessMessageSentEvent @event)
        {
            ProcessMessageId = @event.MessageId;
        }

        public void Apply(NewMusicFileEvent @event)
        {
            Files.Add(new File(@event.FullPath));
        }
    }

    public record File(string FullPath);
}
