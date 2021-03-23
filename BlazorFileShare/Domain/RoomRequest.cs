using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFileShare.Domain
{
    public struct RoomRequest
    {
        public RoomRequest(int code,HttpContext context)
        {
            Code = code;
            Ip = context.Connection.RemoteIpAddress.ToString();
            Ip = "test";
        }

        public string Ip { get; set; }

        public int Code { get; set; }

    }
}
