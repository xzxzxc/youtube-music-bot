using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YoutubeMusicBot.Infrastructure.Database
{
    public class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var dbPathFile = new FileInfo(Path.Join(
                args.FirstOrDefault() ?? Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "messages.db"));

            if (!dbPathFile.Directory!.Exists)
                dbPathFile.Directory.Create();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(
                $"Data Source={dbPathFile.FullName}");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
