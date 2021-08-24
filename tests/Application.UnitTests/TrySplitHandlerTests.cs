using System;
using AutoFixture;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Application.Interfaces.Wrappers;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Models;
using YoutubeMusicBot.Application.Options;
using YoutubeMusicBot.Tests.Common;
using YoutubeMusicBot.Tests.Common.Extensions;

namespace YoutubeMusicBot.UnitTests
{
    [Parallelizable]
    public class TrySplitHandlerTests
    {
        [Test]
        [CustomAutoData]
        public async Task ShouldSplitTracksFromDescription(
            string description,
            TracksList tracks,
            IReadOnlyCollection<IFileInfo> files,
            TrySplitHandler.Request request)
        {
            // TODO: add integration tests for real output
            var descriptionFile = Mock.Of<IFileInfo>(
                f => f.GetTextAsync(It.IsAny<CancellationToken>()).Result == description);
            var newTrackRequests = new List<NewTrackHandler.Request>();
            var container = CreateAutoMock(
                b =>
                {
                    b.RegisterMockOf<IDescriptionService>(
                        s => s.GetFileOrNull(request.File) == descriptionFile);
                    b.RegisterMockOf<ITrackListParser>(
                        p => p.Parse(description) == tracks);
                });
            container.Mock<IMp3SplitWrapper>()
                .Setup(w => w.SplitAsync(request.File, tracks, It.IsAny<CancellationToken>()))
                .Returns(files.ToAsyncEnumerable());

            var sut = container.Create<TrySplitHandler>();

            var res = await sut.Handle(request);

            res.Should().BeTrue();
            container.Mock<IMediator>()
                .Verify(
                    m => m.Send<NewTrackHandler.Request, bool>(
                        Capture.In(newTrackRequests),
                        It.IsAny<CancellationToken>()));
            newTrackRequests.Select(t => t.File).Should().BeEquivalentTo(files);
            newTrackRequests.Should().NotContain(r => r.SkipSplit == false);
        }

        [Test]
        [CustomInlineAutoData]
        public async Task ShouldSplitFileBySilenceIfFileIsTooLarge(
            IReadOnlyCollection<IFileInfo> files)
        {
            var newTrackRequests = new List<NewTrackHandler.Request>();
            var fixture = AutoFixtureFactory.Create();
            var request = fixture.Build<TrySplitHandler.Request>()
                .With(r => r.FileIsTooLarge, true)
                .Create();
            var container = CreateAutoMock();
            container.Mock<IMp3SplitWrapper>()
                .Setup(w => w.SplitBySilenceAsync(request.File, It.IsAny<CancellationToken>()))
                .Returns(files.ToAsyncEnumerable());
            var sut = container.Create<TrySplitHandler>();

            var res = await sut.Handle(request);

            res.Should().BeTrue();
            container.Mock<IMediator>()
                .Verify(
                    m => m.Send<NewTrackHandler.Request, bool>(
                        Capture.In(newTrackRequests),
                        It.IsAny<CancellationToken>()));
            newTrackRequests.Select(t => t.File).Should().BeEquivalentTo(files);
            newTrackRequests.Should().NotContain(r => r.SkipSplit == false);
            container.Mock<ITgClientWrapper>()
                .Verify(
                    w => w.UpdateMessageAsync(
                        "File is to large to be sent in telegram. "
                        + "Trying to get tracks using silence detection.",
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Test]
        [CustomInlineAutoData(7, 3, 3)]
        public async Task ShouldSplitFileInEqualPartsIfNoFilesBySilence(
            long fileLength,
            long maxFileSize,
            int fileParts,
            IReadOnlyCollection<IFileInfo> files)
        {
            var newTrackRequests = new List<NewTrackHandler.Request>();
            var fixture = AutoFixtureFactory.Create();
            var file = Mock.Of<IFileInfo>(f => f.Length == fileLength);
            var request = fixture.Build<TrySplitHandler.Request>()
                .With(r => r.FileIsTooLarge, true)
                .With(f => f.File, () => file)
                .Create();
            var container = CreateAutoMock(
                b =>
                {
                    b.RegisterOptions(new BotOptions { MaxFileBytesCount = maxFileSize, });
                });
            container.Mock<IMp3SplitWrapper>()
                .Setup(w => w.SplitBySilenceAsync(request.File, It.IsAny<CancellationToken>()))
                .Returns(Enumerable.Empty<IFileInfo>().ToAsyncEnumerable());
            container.Mock<IMp3SplitWrapper>()
                .Setup(
                    w => w.SplitInEqualPartsAsync(
                        request.File,
                        fileParts,
                        It.IsAny<CancellationToken>()))
                .Returns(files.ToAsyncEnumerable());
            var sut = container.Create<TrySplitHandler>();

            var res = await sut.Handle(request);

            res.Should().BeTrue();
            container.Mock<IMediator>()
                .Verify(
                    m => m.Send<NewTrackHandler.Request, bool>(
                        Capture.In(newTrackRequests),
                        It.IsAny<CancellationToken>()));
            newTrackRequests.Select(t => t.File).Should().BeEquivalentTo(files);
            newTrackRequests.Should().NotContain(r => r.SkipSplit == false);
            VerifySilenceDetectionFailedMessageSent(container);
        }

        [Test]
        [CustomInlineAutoData(7, 3, 3)]
        public async Task ShouldSplitFileInEqualPartsIfOneFileBySilenceFailed(
            long fileLength,
            long maxFileSize,
            int fileParts,
            IReadOnlyList<IFileInfo> silenceFiles,
            IReadOnlyList<IFileInfo> equalPartsFiles)
        {
            var newTrackRequests = new List<NewTrackHandler.Request>();
            var fixture = AutoFixtureFactory.Create();
            var file = Mock.Of<IFileInfo>(f => f.Length == fileLength);
            var request = fixture.Build<TrySplitHandler.Request>()
                .With(r => r.FileIsTooLarge, true)
                .With(f => f.File, () => file)
                .Create();
            var container = CreateAutoMock(
                b =>
                {
                    b.RegisterOptions(new BotOptions { MaxFileBytesCount = maxFileSize, });
                });
            container.Mock<IMp3SplitWrapper>()
                .Setup(w => w.SplitBySilenceAsync(request.File, It.IsAny<CancellationToken>()))
                .Returns(silenceFiles.ToAsyncEnumerable());
            container.Mock<IMp3SplitWrapper>()
                .Setup(
                    w => w.SplitInEqualPartsAsync(
                        request.File,
                        fileParts,
                        It.IsAny<CancellationToken>()))
                .Returns(equalPartsFiles.ToAsyncEnumerable());
            container.Mock<IMediator>()
                .Setup(
                    m => m.Send<NewTrackHandler.Request, bool>(
                        It.Is<NewTrackHandler.Request>(r => silenceFiles[1] == r.File),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            var sut = container.Create<TrySplitHandler>();

            var res = await sut.Handle(request);

            res.Should().BeTrue();

            container.Mock<IMediator>()
                .Verify(
                    m => m.Send<NewTrackHandler.Request, bool>(
                        Capture.In(newTrackRequests),
                        It.IsAny<CancellationToken>()));
            newTrackRequests.Select(t => t.File).Should().Contain(equalPartsFiles);
            newTrackRequests.Should().NotContain(r => r.SkipSplit == false);
            VerifySilenceDetectionFailedMessageSent(container);
        }

        private static void VerifySilenceDetectionFailedMessageSent(AutoMock container)
        {
            container.Mock<ITgClientWrapper>()
                .Verify(
                    w => w.UpdateMessageAsync(
                        "Silence detection failed. Track would be sent as multiple files.",
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        private static AutoMock CreateAutoMock(Action<ContainerBuilder>? beforeBuild = null) =>
            AutoMockContainerFactory.Create(
                builder =>
                {
                    var mediatorMock = new Mock<IMediator>();
                    mediatorMock
                        .Setup(
                            m => m.Send<NewTrackHandler.Request, bool>(
                                It.IsAny<NewTrackHandler.Request>(),
                                It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);
                    builder.RegisterMock(mediatorMock);
                    beforeBuild?.Invoke(builder);
                });
    }
}
