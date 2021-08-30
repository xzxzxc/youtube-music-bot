using AutoFixture.NUnit3;

namespace YoutubeMusicBot.IntegrationTests.Common.AutoFixture.Attributes
{
    public class CustomInlineAutoDataAttribute : InlineAutoDataAttribute
    {
        public CustomInlineAutoDataAttribute(params object[] values)
            : base(
                AutoFixtureFactory.Create,
                values)
        {
        }
    }
}
