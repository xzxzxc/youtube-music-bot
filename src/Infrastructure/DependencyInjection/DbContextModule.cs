using Autofac;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Infrastructure.Database;

namespace YoutubeMusicBot.Infrastructure.DependencyInjection
{
    public class DbContextModule : Module
    {
        private readonly DbContextFactory _contextFactory;

        public DbContextModule(string? overridePath = null, bool enableSensitiveLogin = false)
        {
            _contextFactory = new DbContextFactory(overridePath, enableSensitiveLogin);
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(_ => _contextFactory.CreateDbContext())
                .As<IDbContext>()
                .AsSelf()
                .As<IInitializable>();
        }
    }
}
