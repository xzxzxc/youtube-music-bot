using System.Reflection;
using Autofac;
using Autofac.Core.Registration;

namespace YoutubeMusicBot.Application.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IModuleRegistrar RegisterApplicationModules(this ContainerBuilder builder) =>
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
    }
}
