using Moq;
using YoutubeMusicBot.IntegrationTests.Common.AutoFixture;

namespace YoutubeMusicBot.IntegrationTests.Common.Moq
{
    public static class MockRepositoryFactory
    {
        public static MockRepository Create() =>
            new(MockBehavior.Loose)
            {
                DefaultValueProvider = new AutoFixtureValueProvider(
                    AutoFixtureFactory.Create()),
            };
    }
}
