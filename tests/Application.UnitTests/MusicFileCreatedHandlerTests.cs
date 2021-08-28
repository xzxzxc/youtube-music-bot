﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;
using YoutubeMusicBot.UnitTests.Extensions;
using CT = System.Threading.CancellationToken;

namespace YoutubeMusicBot.UnitTests
{
    public class MusicFileCreatedHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldRaiseMusicFileCreatedOnEachTrackInDescription(
            MusicFileCreatedEvent @event,
            string description,
            IReadOnlyList<TrackModel> trackModels,
            IReadOnlyList<string> trackFilePaths)
        {
            @event.Aggregate.ClearUncommittedEvents();
            using var container = AutoMockContainerFactory.Create();
            container.Mock<IFileSystem>()
                .Setup(fs => fs.GetFileTextAsync(@event.DescriptionFilePath!, It.IsAny<CT>()))
                .ReturnsAsync(description);
            container.Mock<ITrackListParser>()
                .Setup(p => p.Parse(description))
                .Returns(trackModels);
            container.Mock<IMusicSplitter>()
                .Setup(
                    s => s.SplitAsync(
                        @event.MusicFilePath,
                        It.Is<IReadOnlyList<TrackModel>>(ts => ts.SequenceEqual(trackModels)),
                        It.IsAny<CT>()))
                .Returns(trackFilePaths.ToAsyncEnumerable());
            var sut = container.Create<MusicFileCreatedHandler>();

            await sut.Handle(@event);

            var uncommittedEvents = @event.Aggregate.GetUncommittedEvents();
            var musicFileCreatedEvents = uncommittedEvents.Should()
                .AllBeOfType<MusicFileCreatedEvent>()
                .Which.ToArray();
            musicFileCreatedEvents.Should().HaveSameCount(trackFilePaths);
            foreach (var (trackFilePath, resEvent) in trackFilePaths.Zip(musicFileCreatedEvents))
            {
                resEvent.MusicFilePath.Should().Be(trackFilePath);
                resEvent.DescriptionFilePath.Should().BeNull();
            }

            container.VerifyMessageSaved(@event.Aggregate, Times.Exactly(trackFilePaths.Count));
        }

        [Test]
        [CustomInlineAutoData(0)]
        [CustomInlineAutoData(1)]
        public async Task ShouldNotRaiseMusicFileCreatedIfZeroOrOneTrackInDescription(
            int tracksCount,
            TrackModel trackModel,
            MusicFileCreatedEvent @event,
            string description)
        {
            @event.Aggregate.ClearUncommittedEvents();
            using var container = AutoMockContainerFactory.Create();
            container.Mock<IFileSystem>()
                .Setup(fs => fs.GetFileTextAsync(@event.DescriptionFilePath!, It.IsAny<CT>()))
                .ReturnsAsync(description);
            var trackModels = tracksCount switch
            {
                0 => Array.Empty<TrackModel>(),
                1 => new[] { trackModel },
                _ => throw new ArgumentOutOfRangeException(nameof(tracksCount), tracksCount, null)
            };
            container.Mock<ITrackListParser>()
                .Setup(p => p.Parse(description))
                .Returns(trackModels);
            var sut = container.Create<MusicFileCreatedHandler>();

            await sut.Handle(@event);

            var uncommittedEvents = @event.Aggregate.GetUncommittedEvents();

            uncommittedEvents.Should().NotContain(e => e is MusicFileCreatedHandler);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldRaiseFileToBeSentCreatedEventOnNotLargeFile(
            MusicFileCreatedEvent seedEvent,
            long fileBytesCount,
            string fileName)
        {
            var @event = seedEvent with { DescriptionFilePath = null };
            @event.Aggregate.ClearUncommittedEvents();
            using var container = AutoMockContainerFactory.Create(
                b => b.RegisterOptions(new BotOptions { FileBytesLimit = fileBytesCount + 1, }));
            container.Mock<IFileSystem>()
                .Setup(fs => fs.GetFileBytesCount(@event.MusicFilePath))
                .Returns(fileBytesCount);
            container.Mock<IFileSystem>()
                .Setup(fs => fs.GetFileName(@event.MusicFilePath))
                .Returns(fileName);
            var sut = container.Create<MusicFileCreatedHandler>();

            await sut.Handle(@event);

            var resEvent = @event.Aggregate.GetUncommittedEvents()
                .Should()
                .ContainSingle()
                .Which.Should()
                .BeOfType<FileToBeSentCreatedEvent>()
                .Which;
            resEvent.FilePath.Should().Be(@event.MusicFilePath);
            resEvent.Title.Should().Be(fileName);
            container.VerifyMessageSaved(@event.Aggregate);
        }

        [Test]
        [CustomAutoData]
        public async Task ShouldRaiseMusicFileCreatedOnEachTrackSplitBySilence(
            MusicFileCreatedEvent seedEvent,
            IReadOnlyList<string> trackFilePaths,
            long fileBytesCount)
        {
            var @event = seedEvent with { DescriptionFilePath = null };
            @event.Aggregate.ClearUncommittedEvents();
            using var container = AutoMockContainerFactory.Create(
                b => b.RegisterOptions(new BotOptions { FileBytesLimit = fileBytesCount, }));
            container.Mock<IFileSystem>()
                .Setup(fs => fs.GetFileBytesCount(@event.MusicFilePath))
                .Returns(fileBytesCount);
            container.Mock<IMusicSplitter>()
                .Setup(
                    s => s.SplitBySilenceAsync(
                        @event.MusicFilePath,
                        It.IsAny<CT>()))
                .Returns(trackFilePaths.ToAsyncEnumerable());
            var sut = container.Create<MusicFileCreatedHandler>();

            await sut.Handle(@event);

            var uncommittedEvents = @event.Aggregate.GetUncommittedEvents();
            var musicFileCreatedEvents = uncommittedEvents.Should()
                .AllBeOfType<MusicFileCreatedEvent>()
                .Which.ToArray();
            musicFileCreatedEvents.Should().HaveSameCount(trackFilePaths);
            foreach (var (trackFilePath, resEvent) in trackFilePaths.Zip(musicFileCreatedEvents))
            {
                resEvent.MusicFilePath.Should().Be(trackFilePath);
                resEvent.DescriptionFilePath.Should().BeNull();
            }

            container.VerifyMessageSaved(@event.Aggregate, Times.Exactly(trackFilePaths.Count));
        }


        [Test]
        [CustomInlineAutoData(8, 3, 3)]
        public async Task ShouldRaiseMusicFileCreatedOnEachEqualTrackPart(
            long fileBytesCount,
            long fileBytesLimit,
            int tracksCount,
            MusicFileCreatedEvent seedEvent)
        {
            var trackFilePaths = AutoFixtureFactory.Create()
                .CreateMany<string>(tracksCount)
                .ToArray();
            var @event = seedEvent with { DescriptionFilePath = null };
            @event.Aggregate.ClearUncommittedEvents();
            using var container = AutoMockContainerFactory.Create(
                b => b.RegisterOptions(new BotOptions { FileBytesLimit = fileBytesLimit, }));
            container.Mock<IFileSystem>()
                .Setup(fs => fs.GetFileBytesCount(@event.MusicFilePath))
                .Returns(fileBytesCount);
            container.Mock<IMusicSplitter>()
                .Setup(
                    s => s.SplitBySilenceAsync(
                        @event.MusicFilePath,
                        It.IsAny<CT>()))
                .Returns(Enumerable.Empty<string>().ToAsyncEnumerable());
            container.Mock<IMusicSplitter>()
                .Setup(
                    s => s.SplitInEqualPartsAsync(
                        @event.MusicFilePath,
                        tracksCount,
                        It.IsAny<CT>()))
                .Returns(trackFilePaths.ToAsyncEnumerable());
            var sut = container.Create<MusicFileCreatedHandler>();

            await sut.Handle(@event);

            var uncommittedEvents = @event.Aggregate.GetUncommittedEvents();
            var musicFileCreatedEvents = uncommittedEvents.Should()
                .AllBeOfType<MusicFileCreatedEvent>()
                .Which.ToArray();
            musicFileCreatedEvents.Should().HaveSameCount(trackFilePaths);
            foreach (var (trackFilePath, resEvent) in trackFilePaths.Zip(musicFileCreatedEvents))
            {
                resEvent.MusicFilePath.Should().Be(trackFilePath);
                resEvent.DescriptionFilePath.Should().BeNull();
            }

            container.VerifyMessageSaved(@event.Aggregate, Times.Exactly(tracksCount));
        }
    }
}
