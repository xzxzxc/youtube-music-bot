using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Sequences;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Telegram.Bot;
using TLSharp.Core;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Console;
using YoutubeMusicBot.Infrastructure.Database;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace Console.IntegrationTest
{
    [SetUpFixture]
    public class CommonFixture
    {
        public static IHost HostInstance = null!;

        public static DirectoryInfo CacheFolder => new("cache_folder");

        public static TelegramClient TgClient { get; private set; } = null!;

        public static IHostApplicationLifetime HostLifetime { get; private set; } = null!;

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

            HostInstance = Program.CreateHostBuilder()
                .ConfigureContainer<ContainerBuilder>(
                    (_, b) =>
                    {
                        // b.RegisterMock(TgClientWrapperMock);
                        // b.RegisterMock(TgClientMock);
                        b.RegisterOptions(new DownloadOptions
                        {
                            CacheFilesFolderPath = CacheFolder.FullName,
                        });
                        b.RegisterOptions(new BotOptions()
                        {
                            Token = Secrets.BotToken,
                        });
                    })
                .UseSerilog(new LoggerConfiguration().WriteTo.InMemory().CreateLogger())
                .Build();

            RootScope = HostInstance.Services.GetRequiredService<ILifetimeScope>();
            await EnsureDatabaseAsync();

            if (!CacheFolder.Exists)
                CacheFolder.Create();
        }

        private static async Task EnsureDatabaseAsync()
        {
            var identityDbContext = RootScope.Resolve<ApplicationDbContext>();
            await identityDbContext.Database.MigrateAsync();
        }

        public static void CheckNoErrorsLogged()
        {
            InMemorySink.Instance.LogEvents.Should()
                .NotContain(e => e.Level >= LogEventLevel.Error);
        }

        public static void CheckCacheDirectoryIsEmpty()
        {
            CacheFolder.EnumerateFiles("*", SearchOption.AllDirectories)
                .Should()
                .BeEmpty();
        }

        [OneTimeTearDown]
        public static async Task OneTimeTearDown()
        {
            HostInstance.Dispose();
        }
    }
}
