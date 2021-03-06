using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using Bolt.Common.Extensions;
using BookWorm.BooksApi.Infrastructure.StartUpTasks;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace BookWorm.BooksApi
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var builder = new ContainerBuilder();

            builder.Populate(services);

            builder.RegisterAssemblyModules(typeof(Startup).GetTypeInfo().Assembly);
            builder.RegisterModule<Bolt.RequestBus.Autofac.RequestBusModule>();

            var container = builder.Build();

            container.Resolve<IEnumerable<IStartUpTask>>()
                .ForEach(task =>
                {
                    try
                    {
                        task.Run();
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                });

            return container.Resolve<IServiceProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //env.ConfigureNLog("NLog.config");
            //loggerFactory.AddConsole().AddNLog();

            //loggerFactory.CreateLogger<Startup>().LogError("Configuration started");

            app.UseDeveloperExceptionPage();
            if (env.IsDevelopment())
            {

            }

            app.UseStaticFiles("/public");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=index}/{id?}");
            });

            app.Run(context =>
            {
                return context.Response.WriteAsync("Hello world from asp.net");
            });
        }
    }
}