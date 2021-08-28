using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Extensions;
using Infrastructure.IntegrationTests.Helpers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Models.YoutubeDownloader;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Infrastructure;
using YoutubeMusicBot.Infrastructure.DependencyInjection;
using YoutubeMusicBot.Infrastructure.Wrappers;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace Infrastructure.IntegrationTests
{
    [Parallelizable]
    public class MusicSplitterTests
    {
        public static DirectoryInfo CacheFolder = new($"{nameof(MusicSplitterTests)}_tests_cache");

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
            using var container = await CreateContainer();
            var file = await DownloadFile(url, container);
            var sut = container.Create<MusicSplitter>();

            var tracks = await sut.SplitBySilenceAsync(file).ToArrayAsync();

            tracks.Should().NotBeNull();
            tracks.Should().HaveCount(tracksCount);
        }

        [Test]
        [TestCaseSource(nameof(ShouldNotTakeIntoAccountTooShortSilenceData))]
        public async Task ShouldNotTakeIntoAccountTooShortSilence(
            string url,
            int tracksCount,
            TimeSpan minSilenceLength)
        {
            using var container = await CreateContainer(
                b => b.RegisterOptions(new SplitOptions { MinSilenceLength = minSilenceLength, }));
            var file = await DownloadFile(url, container);
            var sut = container.Create<MusicSplitter>();

            var tracks = await sut.SplitBySilenceAsync(file).ToArrayAsync();

            tracks.Should().NotBeNull();
            tracks.Should().HaveCount(tracksCount);
        }

        [Test]
        [CustomInlineAutoData("https://youtu.be/lfgWv3ypEIY", 13)]
        public async Task ShouldSplitByError(
            string url,
            int tracksCount)
        {
            using var container = await CreateContainer();
            var file = await DownloadFile(url, container);
            var sut = container.Create<MusicSplitter>();

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
            using var container = await CreateContainer();
            var file = await DownloadFile(url, container);
            var sut = container.Create<MusicSplitter>();

            var tracks = await sut.SplitInEqualPartsAsync(file, tracksCount).ToArrayAsync();

            tracks.Should().NotBeNull();
            tracks.Should().HaveCount(tracksCount);
        }

        private static ValueTask<AutoMock> CreateContainer(Action<ContainerBuilder>? beforeBuild = null) =>
            AutoMockInfrastructureContainerFactory.Create(
                builder =>
                {
                    builder.RegisterModules(new CommonModule());
                    builder.RegisterInstance(AutoFixtureFactory.Create().Create<MessageContext>());
                    builder.RegisterMockOf<ICacheFolder>(f => f.Value == CacheFolder.Name);
                    builder.RegisterGeneric(typeof(ThrowExceptionLogger<>)).As(typeof(ILogger<>));

                    beforeBuild?.Invoke(builder);
                });

        private static async Task<string> DownloadFile(string url, AutoMock container)
        {
            var youtubeDlWrapper = container.Create<YoutubeDownloader>();
            return await youtubeDlWrapper.DownloadAsync(CacheFolder.FullName, url)
                .OfType<FileLoadedResult>()
                .Select(r => r.MusicFilePath)
                .SingleAsync();
        }

        private static IEnumerable<TestCaseData> ShouldNotTakeIntoAccountTooShortSilenceData()
        {
            yield return new TestCaseData("https://youtu.be/lfgWv3ypEIY", 6, 1.Seconds());
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (CacheFolder.Exists)
                CacheFolder.Delete(recursive: true);
        }
    }
}
