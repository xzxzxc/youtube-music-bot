using System.IO;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Moq;

namespace YoutubeMusicBot.Tests.Common
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
            fixture.Customize(new AutoMoqCustomization());
            fixture.Customize<Unit>(m => m.FromFactory(() => Unit.Value));

            return fixture;
        }
    }
}
