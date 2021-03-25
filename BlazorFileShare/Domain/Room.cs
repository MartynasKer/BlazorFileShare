using BlazorFileShare.Shared.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Domain
{
    public class Room
    {
        public Guid Id { get; set; }
        ConcurrentDictionary<RoomMember,int> Members { get; set; } = new();

        
        public RoomMember AddMember(string connectionId)
        {
            var member = new RoomMember { Name = GenerateName(), ConnectionId = connectionId };
            var exist = Members.Keys.SingleOrDefault(x => x.Name == member.Name);
            while (exist is not null)
            {
                member.Name = GenerateName(exist.Name);
                exist = Members.Keys.SingleOrDefault(x => x.Name == member.Name);
            }
            Members.TryAdd(member, 0);
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
            var memberToRemove = Members.Keys.SingleOrDefault(x => x.ConnectionId == connectionId);
            Members.TryRemove(memberToRemove, out int i);
        }

        public RoomMember GetRoomMember(string name)
        {
            return Members.Keys.SingleOrDefault(x => x.Name == name);
        }

        public List<RoomMember> GetRoomMembers()
        {
            return Members.Keys.ToList();
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
            
            return ((char)((int)Members.Last().Key.Name[0] + 1)).ToString();
        }

    }
}
