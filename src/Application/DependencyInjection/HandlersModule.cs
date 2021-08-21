using System.Linq;
using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using YoutubeMusicBot.Console.Mediatr;

namespace YoutubeMusicBot.Console.Handlers.DependencyInjection
{
    public class HandlersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterMediatR(ThisAssembly);

            builder.RegisterDecorator<IMediator>(
                (context, parameters, instance) =>
                {
                    if (parameters
                        .OfType<MediatorDisposableDecorator.DoNotDecorate>()
                        .Any())
                    {
                        return instance;
                    }

                    return new MediatorDisposableDecorator(
                        context.Resolve<ILifetimeScope>());
                });
        }
    }
}
