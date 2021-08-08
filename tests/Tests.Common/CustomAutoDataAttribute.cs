using AutoFixture.NUnit3;

namespace YoutubeMusicBot.Tests.Common
{
    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute() : base(
            AutoFixtureFactory.Create)
        {
        }
    }
}
