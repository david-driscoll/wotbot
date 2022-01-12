using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.CommandLine;
using Rocket.Surgery.Conventions.DependencyInjection;
using Rocket.Surgery.Hosting;
using wotbot.Operations;

namespace wotbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseRocketBooster(RocketBooster.ForDependencyContext(DependencyContext.Default))
                .ConfigureRocketSurgery(c => c.UseDryIoc().AppendConvention<CommandTest>())
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    class CommandTest : ICommandLineConvention
    {
        public void Register(IConventionContext context, ICommandLineContext commandLineContext)
        {
            commandLineContext.AddCommand<Command>("test");
        }

        class Command
        {
            private readonly IMediator _mediator;

            public Command(IMediator mediator)
            {
                _mediator = mediator;
            }
            public async Task<int> OnExecuteAsync()
            {
                await _mediator.Send(new GetAttendance.Request("Old Blanchy-Horde-Wipes on Trash-0"));
                return 0;
            }
        }
    }
}
