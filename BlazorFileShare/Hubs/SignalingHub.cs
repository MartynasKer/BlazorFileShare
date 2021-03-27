using BlazorFileShare.Domain;
using BlazorFileShare.Extensions;
using BlazorFileShare.Services;
using BlazorFileShare.Shared;
using BlazorFileShare.Shared.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Hubs
{
    public class SignalingHub: Hub
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<SignalingHub> logger;

        public SignalingHub(IRoomService roomService, ILogger<SignalingHub> logger)
        {
            _roomService = roomService;
            this.logger = logger;
            
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Task.Run(()=>_roomService.RemoveMember(Context.ConnectionId));
        }

        public async Task JoinRoomAsync(int roomId)
        {
            var roomRequest = new RoomRequest(roomId, Context.GetHttpContext());
            var room = _roomService.JoinRoom(roomRequest);
            if(room == null)
            {
                await Clients.Caller.SendAsync("SendInfo",
                    new Info("Failed to join room, room with such room number does not exist.", false));
                return;
            }
            var member = room.AddMember(Context.ConnectionId, Context.GetHttpContext());
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());
            await Clients.Caller.SendAsync("ConnectedToRoom", room.Id, member, room.MemberCount);

            await Clients.Caller.SendAsync("SendInfo", new Info("joined room " + roomId, true));
            logger.LogInformation($"Client {Context.ConnectionId} joined room {roomId} from {roomRequest.Ip}");
        }



        public async Task CreateRoomAsync(int roomId)
        {
            var roomRequest = new RoomRequest(roomId, Context.GetHttpContext());
            var room = _roomService.CreateRoom(roomRequest);
            if(room == null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("SendInfo",
                    new Info("Failed to create room, room with such room number already exists in the network.", false));
                return;
            }
            var member = room.AddMember(Context.ConnectionId, Context.GetHttpContext());
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());
            await Clients.Caller.SendAsync("ConnectedToRoom", room.Id, member, room.MemberCount);
            await Clients.Caller.SendAsync("SendInfo", new Info("Room Created", true));
            logger.LogInformation($"Client {Context.ConnectionId} created room {roomId} with {roomRequest.Ip}");
        }

        public async Task SignalIceCandidate(RTCIceCandidate ice, Guid peerId, string toName, Guid roomId)
        {
            if (toName == null)
            {
                return;
            }
            var clientId = _roomService.GetRoomMember(roomId, toName).ConnectionId;
            await Clients.Client(clientId).SendAsync("NewIce", ice, peerId);

        }



        public async Task SendAnswerAsync(AnswerRequest answerRequest,string name, Guid roomId)
        {
            logger.LogInformation($"sending answer to {name}");
            var clientId = _roomService.GetRoomMember(roomId, name).ConnectionId;
            await Clients.Client(clientId).SendAsync("SendAnswer", answerRequest);

        }

        public async Task SendOffersAsync(List<OfferRequest> offers, string name, Guid roomId)
        {
            logger.LogInformation($"sending offers from {name}");

            var roomMembers = _roomService.GetRoomMembers(roomId);
            var room = _roomService.GetRoomById(roomId);
            var member = room.GetRoomMember(name);
            roomMembers.Remove(member);
            for(int i =0; i< offers.Count; i++)
            {
                if(roomMembers[i].ConnectionId == Context.ConnectionId)
                {
                    continue;
                }
                member.Offer = offers[i];
                await Clients.Client(roomMembers[i].ConnectionId).SendAsync("JoinedRoom", member);
            }

            
        }

        public async Task DisconnectFromRoomAsync(Guid roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            var members = _roomService.GetRoomMembers(roomId);
            var member = members.SingleOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _roomService.RemoveMember(Context.ConnectionId);

            await Clients.Group(roomId.ToString()).SendAsync("SendInfo", new Info($"{member.Name} has left the room", false));
        }


    }
}
