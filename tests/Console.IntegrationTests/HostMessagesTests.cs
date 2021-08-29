using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Console.IntegrationTest.Extensions;
using FluentAssertions;
using FluentAssertions.Extensions;
using IntegrationTests.Common;
using NUnit.Framework;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using static Console.IntegrationTest.CommonFixture;

namespace Console.IntegrationTest
{
    public class HostMessagesTests : BaseTests
    {
        private readonly TLInputPeerUser _botUser;

        public HostMessagesTests()
        {
            _botUser = new TLInputPeerUser
            {
                UserId = Secrets.BotUserId, AccessHash = Secrets.BotUserAccessHash
            };
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await DeleteBotChatHistory();
        }

        [Test]
        [Order(0)]
        [Timeout(120_000)] // 2 minutes
        [TestCaseSource(nameof(SimpleTestCases))]
        public async Task ShouldUploadAudioOnEcho(
            string url,
            IReadOnlyCollection<ExpectedTrack> expectedTracks)
        {
            await TgClient.SendMessageAsync(_botUser, url);

            int? lastMessageId = default;

            foreach (var expectedFile in expectedTracks)
            {
                while (true)
                {
                    await Task.Delay(1.Seconds());
                    CheckNoErrorsLogged();

                    var messages = await TgClient.GetHistoryMessages(
                        _botUser,
                        minId: lastMessageId);
                    var message = messages.LastOrDefault();
                    if (message == null)
                        continue;

                    lastMessageId = message.Id;

                    var lastMessageMedia = message.Media as TLMessageMediaDocument;
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

                    break;
                }
            }

            // give some time for all evens to be finished
            await Task.Delay(2.Seconds());
            CheckCacheDirectoryIsEmpty();
            var afterAnswerMessages = await TgClient.GetHistoryMessages(_botUser);
            afterAnswerMessages.Should()
                .NotContain(
                    m => (m.FromId ?? 0) == _botUser.UserId
                        && m.Media == null);
        }

        [Test]
        // this test must be the last one because it changes static Options
        [Order(Int32.MaxValue)]
        [Timeout(120_000)] // 2 minutes
        [TestCaseSource(nameof(TestCasesWithTgLimits))]
        public async Task ShouldUploadAudioOnEchoWithTgLimits(
            string url,
            IReadOnlyCollection<ExpectedTrack> expectedTracks,
            long fileBytesLimit)
        {
            BotOptions.FileBytesLimit = fileBytesLimit;

            await ShouldUploadAudioOnEcho(url, expectedTracks);
        }

        [Test]
        [Order(0)]
        [Timeout(30_000)] // 30 seconds
        [TestCase("https://youtu.be/wuROIJ0tRPU")]
        public async Task ShouldCancelLoadingUsingButton(string url)
        {
            await TgClient.SendMessageAsync(_botUser, url);

            int? lastMessageId = default;

            IEnumerable<TLMessage> messages;
            while (true)
            {
                await Task.Delay(1.Seconds());
                CheckNoErrorsLogged();

                messages = await TgClient.GetHistoryMessages(_botUser, minId: lastMessageId);

                var messageWithCancelMarkup = messages
                    .FirstOrDefault(m => m.ReplyMarkup is TLReplyInlineMarkup);

                if (messageWithCancelMarkup == null)
                    continue;

                var cancelMarkup = (TLReplyInlineMarkup)messageWithCancelMarkup.ReplyMarkup;

                var cancelButton = (TLKeyboardButtonCallback)cancelMarkup.Rows[0].Buttons[0];
                try
                {
                    await TgClient.SendRequestAsync<TLBotCallbackAnswer>(
                        new TLRequestGetBotCallbackAnswer
                        {
                            Peer = _botUser,
                            MsgId = messageWithCancelMarkup.Id,
                            Data = cancelButton.Data,
                        });
                }
                catch (InvalidOperationException ex)
                    when (ex.Message == "BOT_RESPONSE_TIMEOUT")
                {
                    // This is OK. From https://core.telegram.org/api/bots/inline#sending-the-inline-query-result

                    // The user client should display the results obtained during querying in a list,
                    // making sure to handle eventual bot timeouts in the form of a
                    // BOT_RESPONSE_TIMEOUT RPC error, by simply not displaying anything.
                }

                break;
            }

            // give some time for all evens to be finished
            await Task.Delay(3.Seconds());

            CheckNoErrorsLogged();
            messages = await TgClient.GetHistoryMessages(_botUser, minId: lastMessageId);
            messages.Should().NotContain(m => (m.FromId ?? 0) == _botUser.UserId);
            CheckCacheDirectoryIsEmpty();
        }

        public static IEnumerable<TestCaseData> SimpleTestCases()
        {
            yield return new TestCaseData(
                "https://youtu.be/wuROIJ0tRPU",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Бронепоїзд (feat. Довгий Пес)",
                        "Гоня & Довгий Пес",
                        TimeSpan.Parse("00:02:06")))) { TestName = "Simple track", };
            const string secondAuthor = "Ницо Потворно";
            yield return new TestCaseData(
                "https://soundcloud.com/potvorno/sets/kyiv",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Київ",
                        secondAuthor,
                        TimeSpan.Parse("00:01:55")),
                    new ExpectedTrack(
                        "Заспіваю ще",
                        secondAuthor,
                        TimeSpan.Parse("00:03:40")))) { TestName = "SoundCloud playlist", };
            const string thirdAuthor = "VISANCE";
            yield return new TestCaseData(
                "https://www.youtube.com/watch?v=ZtuXNrGeQ9s",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Drake - What's Next",
                        thirdAuthor,
                        TimeSpan.Parse("00:03:00")),
                    new ExpectedTrack(
                        "Drake - Wants and Needs (ft. Lil Baby)",
                        thirdAuthor,
                        TimeSpan.Parse("00:03:13")),
                    new ExpectedTrack(
                        "Drake - Lemon Pepper Freestyle (ft. Rick Ross)",
                        thirdAuthor,
                        TimeSpan.Parse("00:06:21")))) { TestName = "Track list in description", };
            yield return new TestCaseData(
                "https://youtu.be/1PYGkzyz_YM",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Глава 94 \"Gavno\"",
                        "Stepan Glava",
                        TimeSpan.Parse("00:03:30")))) { TestName = "Double quotes in file name", };
            yield return new TestCaseData(
                "https://youtu.be/kqrcUKehT_Y",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Зав'язав / Stage 13",
                        "Глава 94",
                        TimeSpan.Parse("00:03:40")))) { TestName = "Single quotes in file name", };
            yield return new TestCaseData(
                "https://www.youtube.com/watch?v=rJ_rcbUB32Y&list=OLAK5uy_ksq4lX25NiCtiwvwPlG5cK1SvCfkp-Hrc",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Україна Кокаїна",
                        "Remafo",
                        TimeSpan.Parse("00:02:32")))) { TestName = "Youtube playlist", };
        }

        public static IEnumerable<TestCaseData> TestCasesWithTgLimits()
        {
            yield return new TestCaseData(
                "https://youtu.be/Ih-ogf9acqI",
                ImmutableArray.Create(
                    new ExpectedTrack(
                        "Останній танець",
                        "Паліндром",
                        TimeSpan.Parse("00:01:54")),
                    new ExpectedTrack(
                        "Останній танець",
                        "Паліндром",
                        TimeSpan.Parse("00:01:54")),
                    new ExpectedTrack(
                        "Останній танець",
                        "Паліндром",
                        TimeSpan.Parse("00:00:36"))),
                3654400L) { TestName = "Split by silence then into equal parts", };
        }

        [TearDown]
        public override async ValueTask TearDown()
        {
            await base.TearDown();
            await DeleteBotChatHistory();
        }

        private async Task DeleteBotChatHistory()
        {
            await TgClient.SendRequestAsync<TLAffectedHistory>(
                new TLRequestDeleteHistory
                {
                    Peer = _botUser, JustClear = false, // delete for bot also
                });
        }

        public record ExpectedTrack(string Title, string Author, TimeSpan Duration);
    }
}
