using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq.Sequences;
using NUnit.Framework;
using TLSharp.Core;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Console;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace Console.IntegrationTest
{
    [SetUpFixture]
    public class CommonFixture
    {
        private static IHost _hostInstance = null!;
        public static Task HostRunTask = null!;

        public static DirectoryInfo CacheFolder => new("cache_folder");

        public static TelegramClient TgClient { get; private set; } = null!;

        public static ILifetimeScope RootScope { get; private set; } = null!;

        [OneTimeSetUp]
        public static async Task OneTimeSetUp()
        {
            Sequence.ContextMode = SequenceContextMode.Async;

            TgClient = new TelegramClient(Secrets.AppApiId, Secrets.AppApiHash);
            await TgClient.ConnectAsync();

            if (!TgClient.IsUserAuthorized())
                throw new InvalidOperationException(
                    "Please login to telegram running this project as a program from folder with dll.");

            _hostInstance = Program.CreateHostBuilder()
                .ConfigureContainer<ContainerBuilder>(
                    (_, b) =>
                    {
                        b.RegisterOptions(
                            new FileSystemOptions { TempFolderPath = CacheFolder.FullName, });
                        b.RegisterOptions(
                            new FeatureOptions { EsArchitectureEnabled = false, });
                        b.RegisterOptions(new BotOptions { Token = Secrets.BotToken, });
                        b.RegisterGeneric(typeof(ThrowExceptionLogger<>)).As(typeof(ILogger<>));
                    })
                .Build();

            await Program.Initialize(_hostInstance);
            RootScope = _hostInstance.Services.GetRequiredService<ILifetimeScope>();

            HostRunTask = _hostInstance.RunAsync();
        }

        public static void CheckCacheDirectoryIsEmpty()
        {
            CacheFolder.EnumerateFiles("*", SearchOption.AllDirectories)
                .Should()
                .BeEmpty();
        }

        public static void CheckNoErrorsLogged()
        {
            var innerExceptions = ThrowExceptionLogger.Errors
                .Select(e => e.InnerException)
                .Where(e => e != null);

            innerExceptions.Should().BeEmpty();
            ThrowExceptionLogger.Errors.Should().BeEmpty();
        }

        [OneTimeTearDown]
        public static async Task OneTimeTearDown()
        {
            _hostInstance.Dispose();
            TgClient.Dispose();

            if (CacheFolder.Exists)
                CacheFolder.Delete(recursive: true);
        }
    }
}
