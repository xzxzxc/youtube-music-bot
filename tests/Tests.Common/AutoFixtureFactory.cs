using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace YoutubeMusicBot.Tests.Common
{
    public static class AutoFixtureFactory
    {
        public static IFixture Create()
        {
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Customize(new AutoMoqCustomization());

            return fixture;
        }
    }
}
