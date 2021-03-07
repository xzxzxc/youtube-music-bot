using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace YoutubeMusicBot.Tests
{
	public class BotTests
	{
		private Fixture _fixture = null!;

		[SetUp]
		public void Setup()
		{
			_fixture = new Fixture();
			_fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
				.ToList()
				.ForEach(b => _fixture.Behaviors.Remove(b));
			_fixture.Behaviors.Add(new OmitOnRecursionBehavior());
		}

		[Test]
		[TestCase("https://youtu.be/wuROIJ0tRPU")]
		public async Task ShouldUploadAudioOnEcho(string url)
		{
			var clientMock = new Mock<ITelegramBotClient>();
			var botHostedService = new BotHostedService(clientMock.Object);
			await botHostedService.StartAsync(CancellationToken.None);
			var message = _fixture
				.Build<Message>()
				.With(m => m.Text, url)
				.Create();

			clientMock.Raise(
				c => c.OnMessage += null,
				this,
				new MessageEventArgs(message));

			clientMock.Verify(
				c => c.SendAudioAsync(
					message.Chat.Id,
					It.Is<InputOnlineFile>(
						f => f.FileType == FileType.Stream
							&& f.Content != null
							&& f.Content.Length > 0),
					It.IsAny<string>(),
					It.IsAny<ParseMode>(),
					It.IsAny<int>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<bool>(),
					It.IsAny<int>(),
					It.IsAny<IReplyMarkup>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<InputMedia>()));
		}
	}
}
