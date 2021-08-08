using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Infrastructure.Database.Configurations
{
    public class MessageConfiguarations : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
        }
    }
}
