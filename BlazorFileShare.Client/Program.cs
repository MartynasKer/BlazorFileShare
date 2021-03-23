using BlazorFileShare.Client;
using BlazorFileShare.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebRTC;
using System.Text;
using System.Threading.Tasks;

namespace BlazorFileShare.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddTransient<RTCPeerConnection>();
            builder.Services.AddScoped<IHubClient, HubClient>();
            builder.Services.AddScoped<IRTCInterop, RTCInterop>();
            builder.Services.AddScoped<IClientRoomService, ClientRoomService>();
            

            await builder.Build().RunAsync();
        }
    }
}
