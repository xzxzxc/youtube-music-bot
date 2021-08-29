using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using YoutubeMusicBot.Application.Services;
using Module = Autofac.Module;

namespace YoutubeMusicBot.Application.DependencyInjection
{
    public class CallbackDataModule : Module
    {
        private readonly Assembly[] _assembliesToScan;

        public CallbackDataModule(params Assembly[] assembliesToScan)
        {
            _assembliesToScan = assembliesToScan;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<CallbackDataFactory>()
                .WithParameter(
                    new TypedParameter(
                        typeof(IEnumerable<Type>),
                        EventSourcingModule.GetAggregateTypes(_assembliesToScan)))
                .AsImplementedInterfaces();
        }
    }
}
