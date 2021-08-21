using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeMusicBot.Application.Interfaces;

namespace YoutubeMusicBot.Application
{
    public class LinuxPathResolver : ILinuxPathResolver
    {
        private readonly IProcessRunner _processRunner;

        public LinuxPathResolver(IProcessRunner processRunner)
        {
            _processRunner = processRunner;
        }

        public async Task<string> Resolve(
            string currentOsPath,
            CancellationToken cancellationToken)
        {
            if (OperatingSystem.IsLinux())
                return currentOsPath;

            if (!OperatingSystem.IsWindows())
                throw new InvalidOperationException("Current OS is not supported.");

            var result = await _processRunner.RunAsync(
                    new ProcessRunner.Request(
                        ProcessName: "wslpath",
                        WorkingDirectory: ".",
                        Arguments: currentOsPath),
                    cancellationToken)
                .FirstAsync(cancellationToken);

            return result;
        }
    }
}
