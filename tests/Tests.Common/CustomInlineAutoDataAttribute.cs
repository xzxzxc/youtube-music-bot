using AutoFixture.NUnit3;

namespace YoutubeMusicBot.Tests.Common
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
