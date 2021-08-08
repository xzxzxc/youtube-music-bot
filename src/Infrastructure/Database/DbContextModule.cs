using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using YoutubeMusicBot.Application;

namespace YoutubeMusicBot.Infrastructure.Database
{
    public class DbContextModule : Module
    {
        private readonly string? _overridePath;
        private readonly DbContextFactory _contextFactory;

        public DbContextModule(string? overridePath = null)
        {
            _overridePath = overridePath;
            _contextFactory = new DbContextFactory();
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var args = _overridePath == null
                ? Array.Empty<string>()
                : new[] { _overridePath };
            builder.Register(_ => _contextFactory.CreateDbContext(args)).As<IDbContext>().AsSelf();
        }
    }
}
