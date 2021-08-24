using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YoutubeMusicBot.Application.Interfaces;

namespace YoutubeMusicBot.Infrastructure.Database
{
    public class ApplicationDbContext : DbContext,
        IDbContext,
        IInitializable
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // public DbSet<Message> Messages { get; set; } = null!;

        public DbSet<T> GetDbSet<T>()
            where T : class =>
            Set<T>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        int IInitializable.Order => int.MinValue;

        async ValueTask IInitializable.Initialize()
        {
            await Database.MigrateAsync();
        }
    }
}
