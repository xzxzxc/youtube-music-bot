using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq.Sequences;
using NUnit.Framework;
using TLSharp.Core;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Console;
using YoutubeMusicBot.Infrastructure.Options;
using YoutubeMusicBot.IntegrationTests.Common;
using YoutubeMusicBot.IntegrationTests.Common.Extensions;

namespace YoutubeMusicBot.AcceptanceTest
{
    [SetUpFixture]
    public class CommonFixture
    {
        private static IHost _hostInstance = null!;
        public static readonly BotOptions BotOptions = new() { Token = Secrets.BotToken, };
        public static readonly SplitOptions SplitOptions = new();
        public static Task HostRunTask = null!;

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

            _hostInstance = Console.Program.CreateHostBuilder()
                .ConfigureContainer<ContainerBuilder>(
                    (_, b) =>
                    {
                        b.RegisterOptions(
                            new FileSystemOptions { TempFolderPath = TempFolder.FullName, });
                        b.RegisterOptions(BotOptions);
                        b.RegisterOptions(SplitOptions);
                        b.RegisterGeneric(typeof(ThrowExceptionLogger<>)).As(typeof(ILogger<>));
                    })
                .Build();

            await Console.Program.Initialize(_hostInstance);
            RootScope = _hostInstance.Services.GetRequiredService<ILifetimeScope>();

            HostRunTask = _hostInstance.RunAsync();
        }

        // this is property on purpose
        public static DirectoryInfo TempFolder => new("cache_folder");

        public static void CheckCacheDirectoryIsEmpty()
        {
            TempFolder.EnumerateFiles("*", SearchOption.AllDirectories)
                .Should()
                .BeEmpty();
        }

        public static void CheckNoErrorsLogged()
        {
            ThrowExceptionLogger.ThrowIfNotEmpty();
        }

        [OneTimeTearDown]
        public static async Task OneTimeTearDown()
        {
            _hostInstance.Dispose();
            TgClient.Dispose();

            if (TempFolder.Exists)
                TempFolder.Delete(recursive: true);
        }
    }
}
