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

        public ICollection<File> CreatedMusicFiles { get; } = new List<File>();

        public ICollection<File> MusicFilesToBeSent { get; } = new List<File>();

        public bool IsFinished { get; private set; } = false;

        public bool IsCancelled { get; private set; } = false;

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

        public void MusicFileCreated(string fullPath, string? descriptionFilePath)
        {
            RaiseEvent(new MusicFileCreatedEvent(fullPath, descriptionFilePath));
        }

        public void FileToBeSentCreated(string path, string title)
        {
            RaiseEvent(new FileToBeSentCreatedEvent(path, title));
        }

        public void Finished()
        {
            RaiseEvent(new MessageFinishedEvent());
        }

        public void Cancalled()
        {
            RaiseEvent(new MessageCancelledEvent());
        }

        public void Apply(MessageCreatedEvent @event)
        {
            Id = @event.AggregateId;
            ExternalId = @event.ExternalId;
            Text = @event.Text;
            ChatId = @event.ChatId;
        }

        public void Apply(MessageValidEvent _)
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

        public void Apply(MusicFileCreatedEvent @event)
        {
            CreatedMusicFiles.Add(new File(@event.MusicFilePath, @event.DescriptionFilePath));
        }

        public void Apply(FileToBeSentCreatedEvent @event)
        {
            MusicFilesToBeSent.Add(new File(@event.FilePath));
        }

        public void Apply(MessageFinishedEvent _)
        {
            IsFinished = true;
        }

        public void Apply(MessageCancelledEvent _)
        {
            IsCancelled = true;
            Finished();
        }
    }
}
