using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using FakeItEasy;
using FakeItEasy.Creation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.DryIoc;
using Xunit;
using Rocket.Surgery.Extensions.Testing;
using Serilog;
using wotbot.Infrastructure;
using wotbot.Operations;
using Xunit.Abstractions;

namespace wotbot.tests
{
    [UnitTestConvention, ExportConvention]
    class UnitTestConvention : IDryIocConvention
    {
        public IContainer Register(IConventionContext conventionContext, IConfiguration configuration, IServiceCollection services, IContainer container)
        {
            container.RegisterInstance(A.Fake<IBlobContainerClientFactory>(), IfAlreadyRegistered.Replace);
            container.RegisterInstance(A.Fake<ITableClientFactory>(), IfAlreadyRegistered.Replace);
            container.RegisterInstance(A.Fake<IHttpClientFactory>(), IfAlreadyRegistered.Replace);
            return container;
        }
    }
}
