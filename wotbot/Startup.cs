using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace wotbot
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry(options =>
            {
                // Disables adaptive sampling.
                options.EnableAdaptiveSampling = false;
                // options.EnableEventCounterCollectionModule
            });
            services.AddHealthChecks();
            services.AddLogging(z => z.AddApplicationInsights());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseDefaultFiles(new DefaultFilesOptions()
            {
                RequestPath = "/question.gif"
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                //endpoints.Map("/config", context => context.Response.WriteAsync(((IConfigurationRoot) configuration).GetDebugView()));
            });

            app.Run(z =>
            {
                 z.Response.Redirect("/question.gif", true, true);
                 return Task.CompletedTask;
            });
            //
            // app.UseEndpoints(endpoints => { endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); }); });
        }
    }
}
