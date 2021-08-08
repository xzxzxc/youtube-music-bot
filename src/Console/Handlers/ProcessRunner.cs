using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YoutubeMusicBot.Console.Interfaces;

namespace YoutubeMusicBot.Console.Handlers
{
    public class ProcessRunner : IProcessRunner
    {
        private static readonly bool IsWindows = OperatingSystem.IsWindows();

        private readonly ILogger<ProcessRunner> _logger;

        public ProcessRunner(ILogger<ProcessRunner> logger)
        {
            _logger = logger;
        }

        public async IAsyncEnumerable<Line> RunAsync(
            Request request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var _ = _logger.BeginScope(
                "{Process} {Arguments}",
                request.ProcessName,
                string.Join(' ', request.Arguments));

            var processInfo = new ProcessStartInfo
            {
                FileName = IsWindows
                    ? @"wsl.exe"
                    : request.ProcessName,
                WorkingDirectory = request.WorkingDirectory,
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
                processInfo.ArgumentList.Add(request.ProcessName);
            }

            foreach (var argument in request.Arguments)
                processInfo.ArgumentList.Add(argument);

            using var process = new Process { StartInfo = processInfo, };
            process.Start();

            var onCancelTaskSource = new TaskCompletionSource();
            cancellationToken.Register(() => onCancelTaskSource.SetCanceled(cancellationToken));


            Task<string?>? readOutputTask = null;
            Task<string?>? readErrorTask = null;
            while (!cancellationToken.IsCancellationRequested
                && ((readOutputTask == null && !process.StandardOutput.EndOfStream)
                    || (readErrorTask == null && !process.StandardError.EndOfStream)))
            {
                readErrorTask ??= process.StandardError.ReadLineAsync();
                readOutputTask ??= process.StandardOutput.ReadLineAsync();

                var resTask = await Task.WhenAny(
                    readErrorTask,
                    readOutputTask,
                    onCancelTaskSource.Task);

                if (resTask == readErrorTask)
                {
                    var line = readOutputTask.Result;
                    if (!string.IsNullOrEmpty(line))
                    {
                        _logger.LogError("Got {Error}", line); // TODO: do it by flag
                        yield return new(line, IsError: true);
                    }

                    readErrorTask = null;
                }
                else if (resTask == readOutputTask)
                {
                    var line = readOutputTask.Result;
                    if (!string.IsNullOrEmpty(line))
                    {
                        _logger.LogInformation("Got {Output}", line);
                        yield return new(line);
                    }

                    readOutputTask = null;
                }
                else
                {
                    process.Kill();

                    // throws TaskCancelledException
                    await resTask;
                }
            }

            await process.WaitForExitAsync(cancellationToken);
        }

        public record Line(string Value, bool IsError = false)
        {
            public static implicit operator string(Line line) =>
                line.Value;
        }

        public record Request(
            string ProcessName,
            string WorkingDirectory,
            params string[] Arguments);
    }
}
