using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using FakeItEasy;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.DryIoc;
using Rocket.Surgery.Extensions.Testing;
using wotbot.Infrastructure;
using wotbot.Operations;
using Xunit;
using Xunit.Abstractions;

namespace wotbot.tests
{
    public class ExtractDataFromSavedVariablesTests : AutoFakeTest
    {
        [Fact]
        public async Task Test1()
        {
            var fixturePath = Path.GetDirectoryName(typeof(ExtractDataFromSavedVariablesTests).Assembly.Location);
            var fixtureDataPath = Path.Join(fixturePath, "Fixtures/CommunityDKP.lua");

            var clientFactory = ServiceProvider.GetRequiredService<IBlobContainerClientFactory>();
            var containerClient = A.Fake<BlobContainerClient>();
            var blobClient = A.Fake<BlobClient>();
            var response = A.Fake<Response<bool>>();
            A.CallTo(() => clientFactory.CreateClient(A<string>._)).Returns(containerClient);
            A.CallTo(() => containerClient.GetBlobClient(A<string>._)).Returns(blobClient);
            A.CallTo(() => response.Value).Returns(true);
            A.CallTo(() => blobClient.ExistsAsync(A<CancellationToken>._)).Returns(Task.FromResult(response));
            A.CallTo(() => blobClient.OpenReadAsync(A<BlobOpenReadOptions>._, A<CancellationToken>._)).Returns(Task.FromResult<Stream>(File.OpenRead(fixtureDataPath)));
            var result = await ServiceProvider.GetRequiredService<IMediator>().Send(new ExtractDataFromSavedVariables.Request("MyContainer", "MyBlob"));

            result.Should().HaveCount(2);
            var team1 = result.First(z => z.Key.TeamId == "Old Blanchy-Horde-Wipes on Trash-0").Value;
            var team2 = result.First(z => z.Key.TeamId == "Old Blanchy-Horde-Wipes on Trash-1").Value;
            team1.AwardedLoot.Should().HaveCount(0);
            team1.AwardedPoints.Should().HaveCount(0);
            team1.PlayerProfiles.Should().HaveCount(0);
            team2.AwardedLoot.Should().HaveCount(2);
            team2.AwardedPoints.Should().HaveCount(4);
            team2.PlayerProfiles.Should().HaveCount(4);
        }

        public ExtractDataFromSavedVariablesTests(ITestOutputHelper outputHelper) : base(outputHelper, LogLevel.Trace)
        {
            var builder = new ConventionContextBuilder(new Dictionary<object, object?>())
                .EnableConventionAttributes()
                .UseAppDomain(AppDomain.CurrentDomain);
            var context = ConventionContext.From(builder);
            var services = new ServiceCollection().ApplyConventions(context);
            var container = Container;
            foreach (var item in context.Conventions.Get<IDryIocConvention, DryIocConvention>())
            {
                if (item is IDryIocConvention convention)
                {
                    container = convention.Register(context, Configuration, services, container);
                }
                else if (item is DryIocConvention @delegate)
                {
                    container = @delegate(context, Configuration, services, container);
                }
            }

            container.Populate(services);

            container.RegisterInstance(A.Fake<IBlobContainerClientFactory>(), IfAlreadyRegistered.Replace);
            container.RegisterInstance(A.Fake<ITableClientFactory>(), IfAlreadyRegistered.Replace);
            container.RegisterInstance(A.Fake<IHttpClientFactory>(), IfAlreadyRegistered.Replace);
            Populate(container);
        }
    }
}
