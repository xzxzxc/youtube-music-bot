using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YoutubeMusicBot.Domain;
using YoutubeMusicBot.Domain.Base;

namespace YoutubeMusicBot.Infrastructure.Database.Configurations
{
    public class MessageConfigurations : AggregateConfigurationBase<Message>
    {
        public override void Configure(EntityTypeBuilder<EventBase<Message>> builder)
        {
            base.Configure(builder);

            builder.Ignore(c => c.Aggregate);
        }
    }
}
