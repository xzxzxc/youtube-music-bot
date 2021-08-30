using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace YoutubeMusicBot.Application.DependencyInjection
{
    public class ValidatorsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddValidatorsFromAssembly(ThisAssembly);

            builder.Populate(serviceCollection);

            base.Load(builder);
        }
    }
}
