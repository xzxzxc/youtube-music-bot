using System.Reflection;
using Microsoft.EntityFrameworkCore;
using YoutubeMusicBot.Application.Abstractions.Storage;

namespace YoutubeMusicBot.Infrastructure.Database
{
    public class ApplicationDbContext : DbContext, IDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<T> GetDbSet<T>()
            where T : class =>
            Set<T>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
