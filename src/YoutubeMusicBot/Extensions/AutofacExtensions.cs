﻿using Autofac;
using YoutubeMusicBot.Models;

namespace YoutubeMusicBot.Extensions
{
	public static class AutofacExtensions
	{
		public static ILifetimeScope BeginMessageLifetimeScope(
			this ILifetimeScope lifetimeScope,
			MessageContext messageContext) =>
			lifetimeScope.BeginLifetimeScope(
				c => c.RegisterInstance(messageContext));
	}
}