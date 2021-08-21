using System.Reflection;
using Microsoft.EntityFrameworkCore;
using YoutubeMusicBot.Application;
using YoutubeMusicBot.Application.Interfaces;
using YoutubeMusicBot.Domain;

namespace YoutubeMusicBot.Infrastructure.Database
{
    public class ApplicationDbContext : DbContext,
        IDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
