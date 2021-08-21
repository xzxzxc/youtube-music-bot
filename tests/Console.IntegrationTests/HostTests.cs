using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using FluentAssertions.Extensions;
using IntegrationTests.Common;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Serilog.Sinks.InMemory;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using static Console.IntegrationTest.CommonFixture;

namespace Console.IntegrationTest
{
    [Parallelizable]
    public class HostTests
    {
        private readonly Task _hostRunTask;

        public HostTests()
        {
            _hostRunTask = HostInstance.RunAsync();
        }

        [SetUp]
        public void SetUp()
        {
            if (!CacheFolder.Exists)
                CacheFolder.Create();
        }

        [Test]
        [Timeout(2_000)] // 2 sec
        public async Task ShouldGracefullyShutDown()
        {
            var hostLifetime = RootScope.Resolve<IHostApplicationLifetime>();
            await Task.Delay(1.Seconds());
            hostLifetime.StopApplication();

            await _hostRunTask;
        }

        [Test]
        [Timeout(120_000)] // 2 minutes
        [TestCaseSource(nameof(TestCases))]
        public async Task ShouldUploadAudioOnEcho(
            string url,
            IReadOnlyCollection<ExpectedFile> expectedFiles)
        {
            var botUser = new TLInputPeerUser
            {
                UserId = Secrets.BotUserId,
                AccessHash = Secrets.BotUserAuthHash
            };
            await TgClient.SendMessageAsync(botUser, url);

            var history = (TLMessages)await TgClient.GetHistoryAsync(botUser, limit: 1);
            var lastMessage = (TLMessage)history.Messages[0];
            var lastMessageId = lastMessage.Id;

            foreach (var expectedFile in expectedFiles)
            {
                while (true)
                {
                    await Task.Delay(1.Seconds());

                    var historySlice = (TLMessagesSlice)await TgClient.GetHistoryAsync(
                        botUser,
                        minId: lastMessageId);
                    lastMessageId = (historySlice.Messages.FirstOrDefault() as TLMessage)?.Id
                        ?? lastMessageId;

                    var res = CheckMessageCameIn(historySlice, expectedFile);
                    if (res)
                        break;
                }
            }

            CheckNoErrorsLogged();
            CheckCacheDirectoryIsEmpty();
        }

        private static bool CheckMessageCameIn(TLMessagesSlice historySlice, ExpectedFile expectedFile)
        {
            foreach (var message in historySlice.Messages)
            {
                var lastMessageMedia = (message as TLMessage)?.Media as TLMessageMediaDocument;
                if (lastMessageMedia == null)
                    continue;

                var document = (TLDocument)lastMessageMedia.Document;
                var audioAttribute = document.Attributes
                    .OfType<TLDocumentAttributeAudio>()
                    .FirstOrDefault();
                if (audioAttribute == null)
                    continue;

                audioAttribute.Title.Should().Be(expectedFile.Title);
                audioAttribute.Performer.Should().Be(expectedFile.Author);
                audioAttribute.Duration.Should().Be((int)expectedFile.Duration.TotalSeconds);

                return true;
            }

            return false;
        }


        [Test]
        [Timeout(30_000)] // 30 seconds
        [TestCase("https://youtu.be/wuROIJ0tRPU")]
        public async Task ShouldCancelLoadingUsingButton(string url)
        {
            // TODO: rewrite
            // var message = FixtureInstance
            //     .Build<Message>()
            //     .With(m => m.Text, url)
            //     .Create();
            // var messageUpdate = FixtureInstance.Build<Update>()
            //     .OmitAutoProperties()
            //     .With(c => c.Id)
            //     .With(c => c.Message, message)
            //     .Create();
            // var completionSource = new TaskCompletionSource<InlineButton?>();
            //
            // TgClientMock.Setup(
            //         c => c.GetUpdatesAsync(
            //             It.IsAny<int>(),
            //             It.IsAny<int>(),
            //             It.IsAny<int>(),
            //             It.IsAny<IEnumerable<UpdateType>>(),
            //             It.IsAny<CancellationToken>()))
            //     .ReturnsAsync(new[] { messageUpdate });
            // TgClientWrapperMock.Setup(
            //         m => m.SendMessageAsync(
            //             It.Is<string>(s => s.StartsWith("Loading")),
            //             It.IsAny<InlineButtonCollection>(),
            //             It.IsAny<CancellationToken>()))
            //     .Callback<string, InlineButtonCollection, CancellationToken>(
            //         (_, ib, _) => completionSource.SetResult(ib.First()))
            //     .ReturnsAsync(FixtureInstance.Create<MessageModel>());
            //
            // var sendTask = _botUpdatesProcessor.ProcessUpdatesAsync();
            //
            // var inlineButton = await completionSource.Task;
            //
            //
            // var callbackQuery = FixtureInstance
            //     .Build<CallbackQuery>()
            //     .With(m => m.Message, message)
            //     .With(m => m.Data, inlineButton?.CallbackData)
            //     .Create();
            // var callbackUpdate = FixtureInstance.Build<Update>()
            //     .OmitAutoProperties()
            //     .With(c => c.Id)
            //     .With(c => c.CallbackQuery, callbackQuery)
            //     .Create();
            //
            // TgClientMock.Setup(
            //         c => c.GetUpdatesAsync(
            //             It.IsAny<int>(),
            //             It.IsAny<int>(),
            //             It.IsAny<int>(),
            //             It.IsAny<IEnumerable<UpdateType>>(),
            //             It.IsAny<CancellationToken>()))
            //     .ReturnsAsync(new[] { callbackUpdate });
            //
            // await _botUpdatesProcessor.ProcessUpdatesAsync();
            // await FluentActions.Awaiting(() => sendTask)
            //     .Should()
            //     .ThrowAsync<OperationCanceledException>();
            //
            // InMemorySink.Instance.LogEvents.Should()
            //     .NotContain(e => e.Level >= LogEventLevel.Error);
            // CheckDirectoryIsEmpty(message.Chat.Id); // TODO: fix this
        }

        public static IEnumerable<TestCaseData> TestCases()
        {
            yield return new TestCaseData(
                "https://youtu.be/wuROIJ0tRPU",
                ImmutableArray.Create(
                    new ExpectedFile(
                        "Бронепоїзд (feat. Довгий Пес)",
                        "Гоня & Довгий Пес",
                        TimeSpan.Parse("00:02:06")))) { TestName = "Simple track", };
            const string secondAuthor = "Ницо Потворно";
            yield return new TestCaseData(
                "https://soundcloud.com/potvorno/sets/kyiv",
                ImmutableArray.Create(
                    new ExpectedFile(
                        "Київ",
                        secondAuthor,
                        TimeSpan.Parse("00:01:55")),
                    new ExpectedFile(
                        "Заспіваю ще",
                        secondAuthor,
                        TimeSpan.Parse("00:03:40")))) { TestName = "SoundCloud playlist", };
            const string thirdAuthor = "VISANCE";
            yield return new TestCaseData(
                "https://www.youtube.com/watch?v=ZtuXNrGeQ9s",
                ImmutableArray.Create(
                    new ExpectedFile(
                        "Drake - What's Next",
                        thirdAuthor,
                        TimeSpan.Parse("00:03:00")),
                    new ExpectedFile(
                        "Drake - Wants and Needs (ft. Lil Baby)",
                        thirdAuthor,
                        TimeSpan.Parse("00:03:13")),
                    new ExpectedFile(
                        "Drake - Lemon Pepper Freestyle (ft. Rick Ross)",
                        thirdAuthor,
                        TimeSpan.Parse("00:06:21")))) { TestName = "Track list in description", };
            yield return new TestCaseData(
                "https://youtu.be/1PYGkzyz_YM",
                ImmutableArray.Create(
                    new ExpectedFile(
                        "Глава 94 \"Gavno\"",
                        "Stepan Glava",
                        TimeSpan.Parse("00:03:30")))) { TestName = "Double quotes in file name", };
            yield return new TestCaseData(
                "https://youtu.be/kqrcUKehT_Y",
                ImmutableArray.Create(
                    new ExpectedFile(
                        "Зав'язав / Stage 13",
                        "Глава 94",
                        TimeSpan.Parse("00:03:40")))) { TestName = "Single quotes in file name", };
        }

        [TearDown]
        public static void TearDown()
        {
            if (CacheFolder.Exists)
                CacheFolder.Delete(recursive: true);

            InMemorySink.Instance.Dispose(); // this would clear all events
        }

        public record ExpectedFile(string Title, string Author, TimeSpan Duration);
    }
}
