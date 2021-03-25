using BlazorFileShare.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFileShare.Services
{

    public class RoomService : IRoomService
    {
        readonly ConcurrentDictionary<RoomRequest, Room> rooms = new();

        public void RemoveMember(string ConnectionId)
        {
            rooms.Values.ToList().ForEach(x => x.RemoveMember(ConnectionId));
            var emptyRooms = rooms.Where(x => x.Value.MemberCount == 0);
            foreach(var room in emptyRooms)
            {
                rooms.TryRemove(room);
            }

        }

        public Room CreateRoom(RoomRequest roomRequest)
        {
            var newRoom = new Room() { Id = Guid.NewGuid() };
            if (rooms.TryAdd(roomRequest, newRoom))
            {
                
                return newRoom;
            }
            return null;
        }

        public Room GetRoomById(Guid roomId)
        {
            return rooms.Values.SingleOrDefault(x => x.Id == roomId);
        }

        public RoomMember GetRoomMember(Guid roomId, string name)
        {
            return rooms.Values.SingleOrDefault(x => x.Id == roomId)?.GetRoomMember(name);
        }

        public List<RoomMember> GetRoomMembers(Guid roomId)
        {
            return GetRoomById(roomId).GetRoomMembers();
        }

        public Room JoinRoom(RoomRequest roomRequest)
        {
            
            if(rooms.TryGetValue(roomRequest, out var room))
            {
                return room;
            }
            return null;
        }


    }
}
