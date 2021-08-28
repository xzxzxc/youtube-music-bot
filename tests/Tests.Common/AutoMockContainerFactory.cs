using System;
using Autofac;
using Autofac.Extras.Moq;
using Moq;

namespace YoutubeMusicBot.Tests.Common
{
    public static class AutoMockContainerFactory
    {
        public static AutoMock Create(bool verifyAllMocks = true) =>
            Create((_, _) => { }, verifyAllMocks);

        public static AutoMock Create(
            Action<ContainerBuilder> beforeBuild,
            bool verifyAllMocks = true) =>
            Create((_, buider) => beforeBuild(buider), verifyAllMocks);

        public static AutoMock Create(
            Action<MockRepository, ContainerBuilder> beforeBuild,
            bool verifyAllMocks = true)
        {
            var mockRepository = MockRepositoryFactory.Create();

            var res = AutoMock.GetFromRepository(
                mockRepository,
                builder => beforeBuild(mockRepository, builder));
            res.VerifyAll = verifyAllMocks;
            return res;
        }
    }
}
