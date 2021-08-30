using AutoFixture.NUnit3;

namespace YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes
{
    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute() : base(
            AutoFixtureFactory.Create)
        {
        }
    }
}
