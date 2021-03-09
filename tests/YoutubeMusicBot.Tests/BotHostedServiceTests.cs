using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
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
	public class BotHostedServiceTests
	{
		private Fixture _fixture = null!;
		private AutoMock _mock = null!;
		private Mock<ITelegramBotClient> _clientMock = null!;

		[SetUp]
		public void Setup()
		{
			_fixture = new Fixture();
			_fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
				.ToList()
				.ForEach(b => _fixture.Behaviors.Remove(b));
			_fixture.Behaviors.Add(new OmitOnRecursionBehavior());

			_clientMock = new Mock<ITelegramBotClient>();

			_mock = AutoMock.GetLoose(
				b =>
				{
					Program.ConfigureContainer(null, b);

					// replaces real impl
					b.RegisterGeneric(typeof(OptionsMonitorStub<>))
						.As(typeof(IOptionsMonitor<>));
					b.RegisterMock(_clientMock);
				});
		}

		[Test]
		[TestCase("https://youtu.be/wuROIJ0tRPU")]
		public async Task ShouldUploadAudioOnEcho(string url)
		{
			var contentLength = default(long?);
			var message = _fixture
				.Build<Message>()
				.With(m => m.Text, url)
				.Create();
			var botHostedService = _mock.Create<BotHostedService>();
			_clientMock.Setup(
					m => m.SendAudioAsync(
						It.Is<ChatId>(cId => cId.Identifier == message.Chat.Id),
						It.IsAny<InputOnlineFile>(),
						default,
						default,
						default,
						default,
						default,
						default,
						default,
						default,
						default,
						default))
				.Callback<ChatId, InputOnlineFile, string, ParseMode, int,
					string, string, bool, int, IReplyMarkup, CancellationToken,
					InputMedia>(
					(
						_,
						audio,
						_,
						_,
						_,
						_,
						_,
						_,
						_,
						_,
						_,
						_) =>
					{
						// save length because stream would be disposed
						contentLength = audio.Content?.Length;
					})
				.ReturnsAsync(default(Message));
			await botHostedService.ProcessClientMessageAsync(
				new MessageEventArgs(message));

			_clientMock.VerifyAll();
			contentLength.Should().BeGreaterThan(0);
		}

		[TearDown]
		public async Task TearDown()
		{
			Directory.Delete(
				new DownloadOptions().CacheFilesFolderPath,
				recursive: true);
		}
	}
}
