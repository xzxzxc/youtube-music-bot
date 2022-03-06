using System.IO;
using AutoFixture;

namespace YoutubeMusicBot.IntegrationTests.Common
{
    public static class TempFolderFactory
    {
        private static readonly Fixture Fixture = new();

        public static DirectoryInfo Create() =>
            new(Path.Join("tmp", Fixture.Create<string>()));
    }
}
