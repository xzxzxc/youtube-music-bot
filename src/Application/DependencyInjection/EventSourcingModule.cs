using System.Linq;
using Autofac;
using YoutubeMusicBot.Application.EventSourcing;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Application.DependencyInjection
{
    public class EventSourcingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterGeneric(typeof(EventSourcingRepository<>))
                .As(typeof(IRepository<>))
                .AsSelf();

            var aggregateTypes = typeof(AggregateBase<>).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(
                    t => t.BaseType != null
                        && t.BaseType.IsGenericType
                        && t.BaseType.GetGenericTypeDefinition() == typeof(AggregateBase<>));
            foreach (var aggregateType in aggregateTypes)
            {
                var repoType = typeof(EventSourcingRepository<>).MakeGenericType(aggregateType);
                builder.Register(ctx => ctx.Resolve(repoType))
                    .As<IInitializable>();
            }
        }
    }
}
