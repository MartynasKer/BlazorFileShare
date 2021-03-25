using BlazorFileShare.Domain;
using BlazorFileShare.Shared;
using BlazorFileShare.Shared.Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Services
{
    public class HubClient : IHubClient
    {
        private readonly Microsoft.AspNetCore.SignalR.Client.HubConnection _hub;


        public HubClient(NavigationManager navigationManager)
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/hub"))
                .WithAutomaticReconnect()
                .Build();
            _hub = hubConnection;
            
            _hub.Reconnected += OnConnectionStateChanged;
            _hub.Closed += InvokeConnectionStateChanged;
            _hub.Reconnecting += InvokeConnectionStateChanged;
            StartConnection();
            _hub.On<RoomMember>("JoinedRoom", async(arg)=> await OnPeerJoinedRoom(arg));
            _hub.On<AnswerRequest>("SendAnswer", async (arg) => await OnAnswerReceived(arg));
            _hub.On<RTCIceCandidate, Guid>("NewIce", async (arg1, arg2) => await OnIceCandidate(arg1, arg2));


        }

        public HubConnectionState HubConnectionState => _hub.State;

        public event Action<Info> OnInfo;
        public event Func<RoomMember, Task> OnPeerJoinedRoom;
        public event Func<AnswerRequest, Task> OnAnswerReceived;
        public event Func<RTCIceCandidate, Guid, Task> OnIceCandidate;
        public event Func<string, Task> OnConnectionStateChanged;
      
    

        public async Task CreateRoomAsync(int roomCode, Action<Guid, string, int> returnAction)
        {
            _hub.Remove("ConnectedToRoom");
            await _hub.SendAsync("CreateRoomAsync", roomCode);
            _hub.On("ConnectedToRoom", returnAction);
            _hub.On("SendInfo", OnInfo);
        }

        protected async Task InvokeConnectionStateChanged(Exception e)
        {
            await OnConnectionStateChanged.Invoke(e.Message);
        }

        protected async void StartConnection()
        {
            await _hub.StartAsync();
            await OnConnectionStateChanged.Invoke("Connected to hub");
        }

        public async Task DisconnectFromRoomAsync(Guid roomId)
        {
            await _hub.SendAsync("DisconnectFromRoomAsync", roomId);
        }

        public async Task JoinRoomAsync(int roomCode, Action<Guid, string, int> returnAction)
        {
            _hub.Remove("ConnectedToRoom");
            await _hub.SendAsync("JoinRoomAsync", roomCode);
            _hub.On("ConnectedToRoom", returnAction);
            _hub.On("SendInfo", OnInfo);
        }

        public async Task SignalIceAsync(string name, Guid peerId,RTCIceCandidateInit e, Guid roomId)
        {
            await _hub.SendAsync("SignalIceCandidate", e.candidate, peerId, name, roomId);

        }
        public async Task SendAnswerAsync(AnswerRequest request, string name, Guid roomId)
        {
            await _hub.SendAsync("SendAnswerAsync", request, name, roomId);
            
        }

        public async Task SendOffersAsync(List<OfferRequest> offers, string fromName, Guid roomId)
        { 
            await _hub.SendAsync("SendOffersAsync", offers, fromName, roomId);
        }

        public async Task ReconectAsync()
        {
            await _hub.StartAsync();
            await OnConnectionStateChanged.Invoke("Connected to hub");
        }
    }
}
