﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using YoutubeMusicBot.Tests.Common;
using static Console.IntegrationTest.CommonFixture;

namespace Console.IntegrationTest
{
    public class BaseTests
    {
        [SetUp]
        public virtual async ValueTask SetUp()
        {
            ThrowExceptionLogger.Errors.Clear();

            if (!CacheFolder.Exists)
                CacheFolder.Create();
        }

        [TearDown]
        public virtual async ValueTask TearDown()
        {
            var filesFromPrevRun = CacheFolder.Exists
                ? CacheFolder.EnumerateFiles(
                    "*",
                    SearchOption.AllDirectories)
                : Enumerable.Empty<FileInfo>();
            foreach (var fileInfo in filesFromPrevRun)
                fileInfo.Delete();
        }
    }
}