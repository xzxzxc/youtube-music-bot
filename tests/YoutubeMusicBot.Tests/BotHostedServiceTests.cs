using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Tests
{
	public class BotHostedServiceTests
	{
		private Fixture _fixture = null!;
		private AutoMock _mock = null!;
		private Mock<ITgClientWrapper> _clientWrapperMock = null!;

		[SetUp]
		public void Setup()
		{
			_fixture = new Fixture();
			_fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
				.ToList()
				.ForEach(b => _fixture.Behaviors.Remove(b));
			_fixture.Behaviors.Add(new OmitOnRecursionBehavior());

			_clientWrapperMock =
				new Mock<ITgClientWrapper>(MockBehavior.Strict);

			_mock = AutoMock.GetLoose(
				b =>
				{
					Program.ConfigureContainer(null, b);

					// replaces real impl
					b.RegisterGeneric(typeof(OptionsMonitorStub<>))
						.As(typeof(IOptionsMonitor<>));
					b.RegisterMock(_clientWrapperMock);
				});
		}

		[Test]
		[TestCase(
			"https://youtu.be/wuROIJ0tRPU",
			"NA-Гоня & Довгий Пес - Бронепоїзд.mp3")]
		[TestCase(
			"https://soundcloud.com/potvorno/sets/kyiv",
			"1-Київ.mp3",
			"2-Заспіваю ще.mp3")]
		public async Task ShouldUploadAudioOnEcho(
			string url,
			params string[] fileNames)
		{
			var message = _fixture
				.Build<Message>()
				.With(m => m.Text, url)
				.Create();
			var sequence = new MockSequence();
			foreach (var fileName in fileNames)
				SetupSendAudioCall(fileName);
			var messageHandler = _mock.Create<MessageHandler>();

			await messageHandler.Handle(new MessageHandler.Message(message));
			// wait for events
			await Task.Delay(TimeSpan.FromMilliseconds(500));

			_mock.Mock<ILogger<NewTrackHandler.Notification>>()
				.Verify(
					l => l.Log(
						It.Is<LogLevel>(ll => ll >= LogLevel.Error),
						It.IsAny<EventId>(),
						It.IsAny<It.IsAnyType>(),
						It.IsAny<Exception>(),
						It.IsAny<Func<It.IsAnyType, Exception, string>>()),
					Times.Never);

			void SetupSendAudioCall(string fileName)
			{
				_clientWrapperMock
					.InSequence(sequence)
					.Setup(
						m => m.SendAudioAsync(
							It.Is<ChatContext>(
								cId =>
									cId.Id == message.Chat.Id),
							It.Is<FileInfo>(
								f =>
									f.Length > 0 && f.Name == fileName)))
					.ReturnsAsync(new Message());
			}
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
