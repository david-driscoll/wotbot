using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DryIoc.Microsoft.DependencyInjection;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.DryIoc;
using Rocket.Surgery.Extensions.Testing;
using wotbot.Operations;
using Xunit;
using Xunit.Abstractions;

namespace wotbot.tests
{
    public class ExtractProfessionDataTests : AutoFakeTest
    {
        [Fact]
        public async Task Test1()
        {
            var fixturePath = Path.GetDirectoryName(typeof(ExtractDataFromSavedVariablesTests).Assembly.Location);
            var fixtureDataPath = Path.Join(fixturePath, "Fixtures/professiondata-1.txt");
            var fixtureContent = await File.ReadAllTextAsync(fixtureDataPath);
            var result = await ServiceProvider.GetRequiredService<IMediator>().Send(new ExtractProfessionDataFromMessage.Request(fixtureContent));
            result.Should().HaveCount(1);
            var first = result.First();
            first.Player.Should().Be("Sithy");
            first.Profession.Should().Be("Jewelcrafting");
            first.Items.Should().HaveCount(122);
        }

        [Fact]
        public async Task Test2()
        {
            var fixturePath = Path.GetDirectoryName(typeof(ExtractDataFromSavedVariablesTests).Assembly.Location);
            var fixtureDataPath = Path.Join(fixturePath, "Fixtures/professiondata-2.txt");
            var fixtureContent = await File.ReadAllTextAsync(fixtureDataPath);
            var result = await ServiceProvider.GetRequiredService<IMediator>().Send(new ExtractProfessionDataFromMessage.Request(fixtureContent));
            result.Should().HaveCount(2);
            var first = result.First();
            first.Player.Should().Be("Sithy");
            first.Profession.Should().Be("Jewelcrafting");
            first.Items.Should().HaveCount(122);
            var second = result.Skip(1).First();
            second.Player.Should().Be("Sithy");
            second.Profession.Should().Be("Enchanting");
            second.Items.Should().HaveCount(124);
        }

        public ExtractProfessionDataTests(ITestOutputHelper outputHelper) : base(outputHelper, LogLevel.Trace)
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
            Populate(container);
        }
    }
}
