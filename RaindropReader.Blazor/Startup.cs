using Majorsoft.Blazor.Components.Common.JsInterop;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaindropReader.Blazor.Services;
using RaindropReader.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaindropReader.Blazor
{
    public class Startup
    {
        public const string AppRoute = "/app";
        public const string AppItemRoute = "/app/{item}";
        public static string GetAppItemRoute(string item) => $"/app/{item}";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddJsInteropExtensions();

            services.AddScoped<INavigationHandler, NavigationHandler>();
            services.AddScoped<ReaderService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                //Redirect from "/" to "/app".
                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect(AppRoute);
                    return Task.CompletedTask;
                });

                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host"); //TODO don't add as fallback
            });
        }
    }
}
