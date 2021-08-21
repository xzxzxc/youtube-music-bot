﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Infrastructure.DependencyInjection;
using YoutubeMusicBot.Infrastructure.Wrappers;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace Infrastructure.IntegrationTests
{
    [Parallelizable]
    public class Mp3SplitWrapperTests
    {
        public static DirectoryInfo CacheFolder = new($"{nameof(Mp3SplitWrapperTests)}_tests_cache");

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!CacheFolder.Exists)
                CacheFolder.Create();
        }

        [Test]
        [CustomInlineAutoData("https://youtu.be/lfgWv3ypEIY", 13)]
        public async Task ShouldSplitBySilence(
            string url,
            int tracksCount)
        {
            var container = CreateContainer();
            var file = await DownloadFile(url, container);
            var sut = container.Create<Mp3SplitWrapper>();

            var tracks = await sut.SplitBySilenceAsync(file).ToArrayAsync();

            tracks.Should().NotBeNull();
            tracks.Should().HaveCount(tracksCount);
        }

        [Test]
        [CustomInlineAutoData("https://youtu.be/wuROIJ0tRPU", 6)]
        public async Task ShouldSplitInEqualParts(
            string url,
            int tracksCount,
            MessageContext context)
        {
            var container = CreateContainer();
            var file = await DownloadFile(url, container);
            var sut = container.Create<Mp3SplitWrapper>();

            var tracks = await sut.SplitInEqualPartsAsync(file, tracksCount).ToArrayAsync();

            tracks.Should().NotBeNull();
            tracks.Should().HaveCount(tracksCount);
        }

        private static AutoMock CreateContainer() =>
            AutoMockContainerFactory.Create(
                b =>
                {
                    b.RegisterModules(new CommonModule());
                    b.RegisterInstance(AutoFixtureFactory.Create().Create<MessageContext>());
                    b.RegisterMockOf<ICacheFolder>(f => f.Value == CacheFolder.Name);
                    b.RegisterGeneric(typeof(ThrowExceptionLogger<>)).As(typeof(ILogger<>));
                });

        private static async Task<IFileInfo> DownloadFile(string url, AutoMock container)
        {
            var youtubeDlWrapper = container.Create<YoutubeDlWrapper>();
            return await youtubeDlWrapper.DownloadAsync(url).SingleAsync();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (CacheFolder.Exists)
                CacheFolder.Delete(recursive: true);
        }
    }
}
