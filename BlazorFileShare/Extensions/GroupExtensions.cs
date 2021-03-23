using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFileShare.Extensions
{
    public static class GroupExtensions
    {
        public static IClientProxy JoinRoom(this IHubCallerClients Clients, Guid Id)
        {
            return Clients.OthersInGroup(Id.ToString());
        }
       
    }
}
