using BlazorFileShare.Domain;
using BlazorFileShare.Shared;
using BlazorFileShare.Shared.Domain;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Services
{
    public interface IHubClient
    {
        Task CreateRoomAsync(int roomCode, Action<Guid, RoomMember, int> returnAction);
        Task JoinRoomAsync(int roomCode, Action<Guid, RoomMember, int> returnAction);
        Task DisconnectFromRoomAsync(Guid roomId);
        Task SendAnswerAsync(AnswerRequest request, string name, Guid roomId);

        Task SendOffersAsync(List<OfferRequest> offers, string fromnName, Guid roomId);
        Task SignalIceAsync(string name, Guid peerId, RTCIceCandidateInit e, Guid roomId);

        event Func<RoomMember, Task> OnPeerJoinedRoom;
        event Action<Info> OnInfo;
        event Func<AnswerRequest, Task> OnAnswerReceived;

        HubConnectionState HubConnectionState { get; }

        event Func<string, Task> OnConnectionStateChanged;
        event Func<RTCIceCandidate, Guid, Task> OnIceCandidate;

        Task ReconectAsync();
    }
}
