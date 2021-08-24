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
        [Timeout(120_000)] // 2 minutes
        [TestCaseSource(nameof(TestCases))]
        public async Task ShouldUploadAudioOnEcho(
            string url,
            IReadOnlyCollection<ExpectedFile> expectedFiles)
        {
            await TgClient.SendMessageAsync(_botUser, url);

            int? lastMessageId = default;

            foreach (var expectedFile in expectedFiles)
            {
                while (true)
                {
                    await Task.Delay(1.Seconds());
                    CheckNoErrorsLogged();

                    var messages = await TgClient.GetHistoryMessages(
                        _botUser,
                        minId: lastMessageId);
                    lastMessageId = messages.FirstOrDefault()?.Id ?? lastMessageId;

                    var res = CheckMessageCameIn(messages, expectedFile);
                    if (res)
                        break;
                }
            }

            CheckCacheDirectoryIsEmpty();
        }

        private static bool CheckMessageCameIn(
            IEnumerable<TLMessage> messages,
            ExpectedFile expectedFile)
        {
            foreach (var message in messages)
            {
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

                return true;
            }

            return false;
        }


        [Test]
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

            messages = await TgClient.GetHistoryMessages(_botUser, minId: lastMessageId);
            messages.Should().NotContain(m => m.FromId == _botUser.UserId);

            // TODO: fix this and rewrite tear down to delete full directory recursive
            // CheckCacheDirectoryIsEmpty();
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

        public record ExpectedFile(string Title, string Author, TimeSpan Duration);
    }
}
