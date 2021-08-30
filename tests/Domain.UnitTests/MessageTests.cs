using FluentAssertions;
using NUnit.Framework;
using YoutubeMusicBot.Domain.Base;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes;

namespace YoutubeMusicBot.Domain.UnitTests
{
    public class MessageTests
    {
        [Test]
        [CustomAutoData]
        public void ShouldRaiseMessageCreatedOnNew(Message sut)
        {
            var @event = sut.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MessageCreatedEvent>()
                .Which;
            VerifyEventAggregateFields(@event, sut);
        }

        [Test]
        [CustomAutoData]
        public void ShouldRaiseMessageValidEvent(Message sut)
        {
            sut.ClearUncommittedEvents();

            sut.Valid();

            var @event = sut.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MessageValidEvent>()
                .Which;
            VerifyEventAggregateFields(@event, sut);
            sut.IsValid.Should().BeTrue();
        }

        [Test]
        [CustomAutoData]
        public void ShouldRaiseMessageInvalidEvent(
            Message sut,
            string validationMessage)
        {
            sut.ClearUncommittedEvents();

            sut.Invalid(validationMessage);

            var @event = sut.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MessageInvalidEvent>()
                .Which;
            VerifyEventAggregateFields(@event, sut);
            @event.ValidationMessage.Should().Be(validationMessage);
            sut.IsValid.Should().BeFalse();
        }

        [Test]
        [CustomAutoData]
        public void ShouldRaiseLoadingProcessMessageSentEvent(
            Message sut,
            int messageId)
        {
            sut.ClearUncommittedEvents();

            sut.LoadingProcessMessageSent(messageId);

            var @event = sut.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<LoadingProcessMessageSentEvent>()
                .Which;
            VerifyEventAggregateFields(@event, sut);
            @event.MessageId.Should().Be(messageId);
            sut.ProcessMessageId.Should().Be(messageId);
        }


        [Test]
        [CustomAutoData]
        public void ShouldRaiseMusicFileCreatedEvent(
            Message sut,
            string fileFullPath,
            string? descriptionFilePath)
        {
            sut.ClearUncommittedEvents();

            sut.MusicFileCreated(fileFullPath, descriptionFilePath);

            var @event = sut.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<MusicFileCreatedEvent>()
                .Which;
            VerifyEventAggregateFields(@event, sut);
            @event.MusicFilePath.Should().Be(fileFullPath);
            @event.DescriptionFilePath.Should().Be(descriptionFilePath);
            sut.CreatedMusicFiles.Should().ContainEquivalentOf(new
            {
                FullPath = fileFullPath,
                DescriptionFilePath = descriptionFilePath,
            });
        }

        private static void VerifyEventAggregateFields(EventBase<Message> @event, Message message)
        {
            @event.Aggregate.Should().Be(message);
            @event.AggregateId.Should().Be(message.Id);
        }
    }
}
