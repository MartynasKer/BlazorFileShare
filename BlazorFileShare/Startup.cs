using BlazorFileShare.Hubs;
using BlazorFileShare.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorFileShare.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddSignalR();
            services.AddRazorPages();
            services.AddLogging();
            services.AddSingleton<IRoomService, RoomService>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();

            }
            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();

            var WasmAssembly = typeof(BlazorFileShare.Client.Program).GetTypeInfo().Assembly;
            var WasmEmbeddedFileProvider = new EmbeddedFileProvider(
                WasmAssembly,
                "BlazorFileShare.Client.wwwroot"
            );
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = WasmEmbeddedFileProvider
            });
            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<SignalingHub>("/hub");
                endpoints.MapFallbackToPage("/_Host");

            });
        }
    }
}
