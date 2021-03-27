using BlazorFileShare.Shared.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlazorFileShare.Domain
{
    public class Room
    {
        public Guid Id { get; set; }
        ConcurrentDictionary<string, RoomMember> Members { get; set; } = new();
        public bool IsMobileDevice(HttpContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"];
            var deviceName = "Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini";
            return Regex.IsMatch(userAgent, deviceName);
        }

        public RoomMember AddMember(string connectionId, Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var member = new RoomMember { Name = GenerateName(), ConnectionId = connectionId };
            if (IsMobileDevice(httpContext))
            {
                member.Type = DeviceType.Mobile;
            }
            else
            {
                member.Type = DeviceType.Desktop;
            }
            var exist = Members.Values.SingleOrDefault(x => x.Name == member.Name);
            while (exist is not null)
            {
                member.Name = GenerateName(exist.Name);
                exist = Members.Values.SingleOrDefault(x => x.Name == member.Name);
            }
            Members.TryAdd(connectionId, member);
            return member;
        }
        public int MemberCount
        {
            get
            {
                return Members.Count;
            }
        }
        public void RemoveMember(string connectionId)
        {
            Members.TryRemove(connectionId, out var member);
        }

        public RoomMember GetRoomMember(string name)
        {
            return Members.Values.SingleOrDefault(x => x.Name == name);
        }

        public List<RoomMember> GetRoomMembers()
        {
            return Members.Values.ToList();
        }

        public string GenerateName(string lastName = null)
        {
            if(Members.IsEmpty)
            {
                return "A";
            }
            if(lastName != null){
                return ((char)((int)lastName[0] + 1)).ToString();
            }
            
            return ((char)((int)Members.Last().Value.Name[0] + 1)).ToString();
        }

    }
}
