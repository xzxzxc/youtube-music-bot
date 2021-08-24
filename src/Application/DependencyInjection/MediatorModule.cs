using System.Linq;
using System.Reflection;
using Autofac;
using YoutubeMusicBot.Application.Mediator;
using YoutubeMusicBot.Application.Mediator.Implementation;
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

            builder.RegisterType<MediatorService>().AsImplementedInterfaces();

            builder.RegisterDecorator<ExceptionLogDecorator, IMediator>();

            builder.RegisterAssemblyTypes(AssembliesToScan)
                .AsClosedTypesOf(typeof(IEventHandler<,>));
            builder.RegisterAssemblyTypes(AssembliesToScan)
                .AsClosedTypesOf(typeof(IRequestHandler<>));
            builder.RegisterAssemblyTypes(AssembliesToScan)
                .AsClosedTypesOf(typeof(IRequestHandler<,>));
        }

        private Assembly[] AssembliesToScan => _assembliesToScan.Append(ThisAssembly).ToArray();
    }
}
