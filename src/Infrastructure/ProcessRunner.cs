using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YoutubeMusicBot.Infrastructure.Abstractions;
using YoutubeMusicBot.Infrastructure.Models.ProcessRuner;

namespace YoutubeMusicBot.Infrastructure
{
    public class ProcessRunner : IProcessRunner
    {
        private static readonly bool IsWindows = OperatingSystem.IsWindows();

        private readonly ILogger<ProcessRunner> _logger;

        public ProcessRunner(ILogger<ProcessRunner> logger)
        {
            _logger = logger;
        }

        public async IAsyncEnumerable<ProcessResultLine> RunAsync(
            ProcessOptions options,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var _ = _logger.BeginScope(
                "{Process} {Arguments}",
                options.ProcessName,
                string.Join(' ', options.Arguments));

            var processInfo = new ProcessStartInfo
            {
                FileName = IsWindows
                    ? @"wsl.exe"
                    : options.ProcessName,
                WorkingDirectory = options.WorkingDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
            };

            if (IsWindows)
            {
                processInfo.ArgumentList.Add("-e");
                processInfo.ArgumentList.Add(options.ProcessName);
            }

            foreach (var argument in options.Arguments)
                processInfo.ArgumentList.Add(argument);

            using var process = new Process { StartInfo = processInfo, };
            process.Start();

            Task<string?> readOutputTask = process.StandardOutput.ReadLineAsync();
            Task<string?> readErrorTask = process.StandardError.ReadLineAsync();
            var outResultNull = false;
            var errResultNull = false;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    process.Kill();
                    cancellationToken.ThrowIfCancellationRequested();
                    break;
                }

                if (outResultNull && errResultNull)
                    break;

                var resTask = await Task.WhenAny(
                    Task.Delay(Timeout.Infinite, cancellationToken),
                    readErrorTask,
                    readOutputTask);

                if (resTask == readErrorTask)
                {
                    var line = readErrorTask.Result;
                    errResultNull = line == null;
                    if (!errResultNull)
                    {
                        if (line!.StartsWith("Warning", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Gor {Warning}", line);
                            yield return new(line, IsError: false);
                        }

                        _logger.LogError("Got {Error}", line);
                        yield return new(line, IsError: true);
                    }

                    readErrorTask = process.StandardError.ReadLineAsync();
                }
                else if (resTask == readOutputTask)
                {
                    var line = readOutputTask.Result;
                    outResultNull = line == null;
                    if (!outResultNull)
                    {
                        _logger.LogInformation("Got {Output}", line);
                        yield return new(line!);
                    }

                    readOutputTask = process.StandardOutput.ReadLineAsync();
                }
                else
                {
                    process.Kill();

                    // throws OperationCancelledException
                    await resTask;
                }

            }

            await process.WaitForExitAsync(cancellationToken);
        }
    }
}
