using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using Autofac.Features.Indexed;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Sequences;
using NUnit.Framework;
using Telegram.Bot.Types;
using YoutubeMusicBot.Extensions;
using YoutubeMusicBot.Interfaces;
using YoutubeMusicBot.Models;
using YoutubeMusicBot.Options;
using YoutubeMusicBot.Tests.Stubs;
using YoutubeMusicBot.Wrappers.Interfaces;

namespace YoutubeMusicBot.Tests
{
	public class BotHostedServiceTests
	{
		private Fixture _fixture = null!;
		private Mock<ITgClientWrapper> _clientWrapperMock = null!;
		private IContainer _container = null!;

		[SetUp]
		public void Setup()
		{
			Sequence.ContextMode = SequenceContextMode.Async;

			_fixture = new Fixture();
			_fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
				.ToList()
				.ForEach(b => _fixture.Behaviors.Remove(b));
			_fixture.Behaviors.Add(new OmitOnRecursionBehavior());

			_clientWrapperMock =
				new Mock<ITgClientWrapper>(MockBehavior.Strict);

			var builder = new ContainerBuilder();
			Program.ConfigureContainer(null, builder);
			// replaces real impl
			builder.RegisterGeneric(typeof(OptionsMonitorStub<>))
				.AsImplementedInterfaces();
			builder.RegisterGeneric(typeof(LoggerStub<>))
				.As(typeof(ILogger<>));
			builder.RegisterMock(_clientWrapperMock);

			_container = builder.Build();
		}

		[Test]
		[TestCase(
			"https://youtu.be/wuROIJ0tRPU",
			"NA-Гоня & Довгий Пес - Бронепоїзд.mp3")]
		[TestCase(
			"https://soundcloud.com/potvorno/sets/kyiv",
			"1-Київ.mp3",
			"2-Заспіваю ще.mp3")]
		[TestCase(
			"https://www.youtube.com/watch?v=ZtuXNrGeQ9s",
			"NA-Drake Scary Hours 2 Full Ep_00m_00s__03m_00s.mp3",
			"NA-Drake Scary Hours 2 Full Ep_03m_00s__06m_13s.mp3",
			"NA-Drake Scary Hours 2 Full Ep_06m_13s__12m_34s_13h.mp3")]
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
			var messageHandler = _container.Resolve<MessageHandler>();

			await messageHandler.Handle(new MessageHandler.Message(message));
			await SequenceFullOrTimeout(
				//sequence,
				TimeSpan.FromSeconds(30));

			LoggerStub<object>.Executions.Should()
				.NotContain(e => e.LogLevel >= LogLevel.Error);
			_clientWrapperMock.Invocations.Should().HaveCount(
				fileNames.Length);
			var watcher = _container.Resolve<IIndex<ChatContext, ITrackFilesWatcher>>()[
				message.Chat.ToContext()];
			Directory.EnumerateFileSystemEntries(watcher.ChatFolderPath)
				.Should()
				.BeEmpty();

			void SetupSendAudioCall(string fileName)
			{
				_clientWrapperMock
					.InSequence(sequence)
					.Setup(
						m => m.SendAudioAsync(
							It.Is<ChatContext>(
								context =>
									context.Id == message.Chat.Id),
							It.Is<FileInfo>(
								f =>
									f.Length > 0 && f.Name == fileName)))
					.ReturnsAsync(new Message());
			}
		}

		private async Task SequenceFullOrTimeout(TimeSpan timeOut)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter(timeOut);

			var allStepsCompleted = false;

			try
			{
				do
				{
					allStepsCompleted = _clientWrapperMock.Setups
						.All(s => s.IsMatched);

					await Task.Delay(
						TimeSpan.FromSeconds(2),
						cancellationTokenSource.Token);
				} while (!allStepsCompleted);
			}
			catch (TaskCanceledException)
			{
			}
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDown()
		{
			await _container.DisposeAsync();
			Directory.Delete(
				new DownloadOptions().CacheFilesFolderPath,
				recursive: true);
		}
	}
}
