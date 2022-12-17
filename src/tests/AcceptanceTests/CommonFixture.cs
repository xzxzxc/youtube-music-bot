using System;
using System.IO;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Moq.Sequences;
using NUnit.Framework;
using TLSharp.Core;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.IntegrationTests.Common;

namespace YoutubeMusicBot.AcceptanceTest
{
    [SetUpFixture]
    public class CommonFixture
    {
        public static readonly BotOptions BotOptions = new() { Token = Secrets.BotToken, };
        private static TestcontainersContainer _container = null!;

        public static TelegramClient TgClient { get; private set; } = null!;


        [OneTimeSetUp]
        public static async Task OneTimeSetUp()
        {
            Sequence.ContextMode = SequenceContextMode.Async;

            TgClient = new TelegramClient(Secrets.AppApiId, Secrets.AppApiHash);
            await TgClient.ConnectAsync();

            if (!TgClient.IsUserAuthorized())
                throw new InvalidOperationException(
                    "Please login to telegram running this project as a program from folder with dll.");

            var imageName = await new ImageFromDockerfileBuilder()
                .WithName("telegram-youtube-music-bot-test")
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath)
                .WithDockerfile("Dockerfile")
                .WithDeleteIfExists(true)
                .Build();

            var output = Consume.RedirectStdoutAndStderrToStream(
                new MemoryStream(),
                new MemoryStream());
            _container = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage(imageName)
                .WithEnvironment("BOT__TOKEN", Secrets.BotToken)
                .WithEnvironment("DOWNLOAD__TempFolderPath", TempFolder.FullName)
                .WithOutputConsumer(output)
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilMessageIsLogged(output.Stdout, "Hosting environment: Production"))
                .Build();
           await _container.StartAsync();
        }

        // this is property on purpose
        public static DirectoryInfo TempFolder => new("cache_folder");

        public static void CheckCacheDirectoryIsEmpty()
        {
            TempFolder.EnumerateFiles("*", SearchOption.AllDirectories)
                .Should()
                .BeEmpty();
        }

        public static async Task CheckNoErrorsLogged()
        {
            var (_, error) = await _container.GetLogs();
            error.Should().BeEmpty();
        }

        [OneTimeTearDown]
        public static async Task OneTimeTearDown()
        {
            await _container.DisposeAsync();
            TgClient.Dispose();

            if (TempFolder.Exists)
                TempFolder.Delete(recursive: true);
        }
    }
}
