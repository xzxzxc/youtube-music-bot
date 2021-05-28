using System.Linq;
using AutoFixture;

namespace YoutubeMusicBot.Tests
{
    public class AutoFixtureFactory
    {
        public static IFixture Create()
        {
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            return fixture;
        }
    }
}
