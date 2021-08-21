using System.Reflection;
using Autofac;
using Autofac.Core.Registration;

namespace YoutubeMusicBot.Console.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static IModuleRegistrar RegisterInfrastructureModules(this ContainerBuilder builder) =>
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
    }
}
