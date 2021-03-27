using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFileShare.Domain
{
    public struct RoomRequest
    {
        public RoomRequest(int code,HttpContext context, bool development=false)
        {
            Code = code;
            Ip = context.Connection.RemoteIpAddress.ToString();
            if(development)
                Ip = "test";
        }

        public string Ip { get; set; }

        public int Code { get; set; }

    }
}
