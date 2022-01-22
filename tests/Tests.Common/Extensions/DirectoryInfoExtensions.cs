using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions.Extensions;

namespace YoutubeMusicBot.IntegrationTests.Common.Extensions;

public static class DirectoryInfoExtensions
{
    public static async ValueTask WaitToDelete(
        this DirectoryInfo directory,
        bool recursive = false)
    {
        while (directory.Exists)
        {
            try
            {
                directory.Delete(recursive);
            }
            catch (IOException)
            {
                // probably some of files is locked, wait for lock to end up
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(25.Milliseconds());
        }
    }
}
