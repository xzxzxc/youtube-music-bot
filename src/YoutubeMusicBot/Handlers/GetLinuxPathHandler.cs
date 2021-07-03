using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace YoutubeMusicBot.Handlers
{
	public class GetLinuxPathHandler : IRequestHandler<GetLinuxPathHandler.Request, string>
	{
		private static readonly bool IsWindows = OperatingSystem.IsWindows();

		private readonly IMediator _mediator;

		public GetLinuxPathHandler(
			IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<string> Handle(Request request,
			CancellationToken cancellationToken)
		{
			if (!IsWindows)
				return request.WindowsPath;

			string? result = null;
			await _mediator.Send(
				new RunProcessHandler.Request(
					"wslpath",
					".",
					async (line, _) => result = line,
					Arguments: request.WindowsPath));

			return result
				?? throw new InvalidOperationException(
					"Got no response from wslpath");
		}

		public record Request(string WindowsPath) : IRequest<string>;
	}
}
