using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.Moq;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Sequences;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using YoutubeMusicBot.Console;
using YoutubeMusicBot.Tests.Common;
using TagFile = TagLib.File;
using static FluentAssertions.FluentActions;

namespace YoutubeMusicBot.IntegrationTests
{
    public class MessageHandlerTests
    {
        
    }
}
