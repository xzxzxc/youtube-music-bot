using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YoutubeMusicBot.Infrastructure.Database
{
    public class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        private readonly string? _overridePath;
        private readonly bool _enableSensitiveLogin;

        public DbContextFactory() // ctor for console EF
        {
        }

        public DbContextFactory(string? overridePath, bool enableSensitiveLogin)
        {
            _overridePath = overridePath;
            _enableSensitiveLogin = enableSensitiveLogin;
        }

        public ApplicationDbContext CreateDbContext(params string[] args)
        {
            var dbPathFile = new FileInfo(Path.Join(
                args.FirstOrDefault() ?? _overridePath ?? Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "messages.db"));

            if (!dbPathFile.Directory!.Exists)
                dbPathFile.Directory.Create();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(
                $"Data Source={dbPathFile.FullName}");

            if (_enableSensitiveLogin)
                optionsBuilder.EnableSensitiveDataLogging();

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
