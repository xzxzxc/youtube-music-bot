using System.Linq;
using System.Reflection;
using Autofac;
using YoutubeMusicBot.Application.Abstractions.Mediator;
using YoutubeMusicBot.Application.Services;
using Module = Autofac.Module;

namespace YoutubeMusicBot.Application.DependencyInjection
{
    public class MediatorModule : Module
    {
        private readonly Assembly[] _assembliesToScan;

        public MediatorModule(params Assembly[] assembliesToScan)
        {
            _assembliesToScan = assembliesToScan;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<Mediator>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(AssembliesToScan)
                .AsClosedTypesOf(typeof(IEventHandler<,>));
            builder.RegisterAssemblyTypes(AssembliesToScan)
                .AsClosedTypesOf(typeof(ICommandHandler<>));
            builder.RegisterAssemblyTypes(AssembliesToScan)
                .AsClosedTypesOf(typeof(IRequestHandler<,>));
        }

        private Assembly[] AssembliesToScan => _assembliesToScan.Append(ThisAssembly).ToArray();
    }
}
