using System;
using Autofac;
using Autofac.Extras.Moq;
using Moq;

namespace YoutubeMusicBot.Tests.Common
{
    public static class AutoMockContainerFactory
    {
        public static AutoMock Create() =>
            Create((_, _) => { });

        public static AutoMock Create(Action<ContainerBuilder> beforeBuild) =>
            Create((_, buider) => beforeBuild(buider));

        public static AutoMock Create(Action<MockRepository, ContainerBuilder> beforeBuild)
        {
            var mockRepository = MockRepositoryFactory.Create();

            return AutoMock.GetFromRepository(
                mockRepository,
                builder => beforeBuild(mockRepository, builder));
        }
    }
}
