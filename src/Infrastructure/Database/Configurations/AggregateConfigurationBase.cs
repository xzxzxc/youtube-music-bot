using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YoutubeMusicBot.Application.Extensions;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Infrastructure.Database.Configurations
{
    public abstract class AggregateConfigurationBase<TAggreagte> :
        IEntityTypeConfiguration<EventBase<TAggreagte>>
        where TAggreagte : AggregateBase<TAggreagte>
    {
        private static readonly Type EventBaseType = typeof(EventBase<TAggreagte>);

        private static readonly IReadOnlyCollection<Type> EventTypes = EventBaseType.Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.IsAssignableTo(EventBaseType))
            .ToArray();

        public virtual void Configure(EntityTypeBuilder<EventBase<TAggreagte>> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
            builder.HasIndex(e => e.AggregateId);

            var discriminatorBuilder = builder.HasDiscriminator<int>("event_type");
            foreach (var eventType in EventTypes)
                discriminatorBuilder.HasValue(eventType, eventType.Name.GetDeterministicHashCode());
            builder.HasIndex("event_type");
        }
    }
}
