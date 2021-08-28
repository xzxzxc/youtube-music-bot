using Moq;

namespace YoutubeMusicBot.Tests.Common
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
