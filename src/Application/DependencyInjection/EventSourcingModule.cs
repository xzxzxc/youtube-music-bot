using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using YoutubeMusicBot.Application.Abstractions;
using YoutubeMusicBot.Application.Abstractions.Storage;
using YoutubeMusicBot.Application.Services;
using YoutubeMusicBot.Domain.Base;
using Module = Autofac.Module;

namespace YoutubeMusicBot.Application.DependencyInjection
{
    public class EventSourcingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterGeneric(typeof(EventSourcingRepository<>))
                .AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(RepositoryInitializer<>))
                .AsSelf();
        }

        public static IEnumerable<Type> GetAggregateTypes(params Assembly[] assembliesToScan)
        {
            var aggregateTypes = assembliesToScan
                .Append(typeof(AggregateBase<>).Assembly)
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(
                    t => t.BaseType != null
                        && t.BaseType.IsGenericType
                        && t.BaseType.GetGenericTypeDefinition() == typeof(AggregateBase<>));
            return aggregateTypes;
        }
    }
}
