using System;
using AutoFixture;
using AutoFixture.Kernel;
using Moq;

namespace YoutubeMusicBot.IntegrationTests.Common.AutoFixture
{
    public class AutoFixtureValueProvider : DefaultValueProvider
    {
        private readonly IFixture _fixture;

        public AutoFixtureValueProvider(IFixture fixture)
        {
            _fixture = fixture;
        }

        protected override object GetDefaultValue(Type type, Mock mock) =>
            _fixture.Create(type, new SpecimenContext(_fixture));
    }
}
