using BlazorFileShare.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFileShare.Services
{
    public interface IRoomService
    {
        Room CreateRoom(RoomRequest roomRequest);

        Room JoinRoom(RoomRequest roomRequest);

        RoomMember GetRoomMember(Guid roomId, string name);

        Room GetRoomById(Guid roomId);
        List<RoomMember> GetRoomMembers(Guid roomId);
        void RemoveMember(string ConnectionId);
    }
}
