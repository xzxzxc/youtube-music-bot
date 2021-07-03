using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace YoutubeMusicBot.Handlers
{
    public delegate Task ProcessDelegate(string line, CancellationToken cancellationToken);

    public class RunProcessHandler : IRequestHandler<RunProcessHandler.Request>
    {
        private static readonly bool IsWindows = OperatingSystem.IsWindows();

        private readonly ILogger<RunProcessHandler> _logger;

        public RunProcessHandler(ILogger<RunProcessHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Unit> Handle(
            Request request,
            CancellationToken cancellationToken = default)
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
                        _logger.LogError("Got {Error}", line);
                        if (request.ProcessError != null)
                            await request.ProcessError(line, cancellationToken);
                    }

                    readErrorTask = null;
                }
                else if (resTask == readOutputTask)
                {
                    var line = readOutputTask.Result;
                    if (!string.IsNullOrEmpty(line))
                    {
                        _logger.LogInformation("Got {Output}", line);
                        await request.ProcessOutput(line, cancellationToken);
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

            return Unit.Value;
        }

        public record Request(
            string ProcessName,
            string WorkingDirectory,
            ProcessDelegate ProcessOutput,
            ProcessDelegate? ProcessError,
            params string[] Arguments) : IRequest
        {
            public Request(
                string ProcessName,
                string WorkingDirectory,
                ProcessDelegate ProcessOutput,
                params string[] Arguments)
                : this(
                    ProcessName,
                    WorkingDirectory,
                    ProcessOutput,
                    null,
                    Arguments)
            {
            }
        }
    }
}
