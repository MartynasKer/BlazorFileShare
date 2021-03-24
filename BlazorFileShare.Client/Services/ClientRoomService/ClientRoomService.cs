using BlazorFileShare.Client.Domain;
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
    public class ClientRoomService : IClientRoomService
    {
        private readonly IHubClient hubClient;

        private readonly IRTCInterop rTCInterop;

        public ClientRoomService(IRTCInterop rTCInterop, IHubClient hubClient)
        {
            this.rTCInterop = rTCInterop;
            this.hubClient = hubClient;
            this.hubClient.OnAnswerReceived += OnReceiveAnswerAsync;
            this.hubClient.OnPeerJoinedRoom += OnPeerJoinedAsync;
            this.hubClient.OnInfo += Inform;
            this.hubClient.OnConnectionStateChanged += InvokeStatusChange;
            this.hubClient.OnIceCandidate += rTCInterop.AddIceAsync;

        }
        public List<string> Clients { get; set; } = new List<string>();
        public string MyName { get; private set; }

        public int RoomCode { get; private set; }

        public bool Connected { get; private set; }

        public HubConnectionState State => hubClient.HubConnectionState;

        private Guid _roomId;

        public Task InvokeStatusChange(string info)
        {
            OnStatusChange.Invoke();
            return Task.CompletedTask;
        }

        public event Action OnStatusChange;

        void Inform(Info info)
        {
            Console.WriteLine(info.Message);
        }
        public async Task CreateRoomAsync(int code)
        {
            await hubClient.CreateRoomAsync(code, OnJoinRoom);
        }


        private void OnRTCConnected(string name)
        {
            Clients.Add(name);
        }

        private async Task OnReceiveAnswerAsync(AnswerRequest request)
        {
            Console.WriteLine("received answer...");
            await rTCInterop.SetSDPAnswerAsync(request, request.Name, OnRTCConnected);
            Clients.Add(request.Name);
            OnStatusChange.Invoke();
        }

        public void SendTestMessage(string message)
        {
            rTCInterop.SendMessageToAll(message);
        }

        private async Task OnPeerJoinedAsync(RoomMember member)
        {
            Console.WriteLine("peer joined");
            await rTCInterop.SetSDPOfferAsync(member.Offer, member.Name, OnIceCandidateAsync, OnRTCConnected);
            var answer = await rTCInterop.CreateSDPAnswerAsync(member.Name, member.Offer.PeerConnectionId);
            answer.Name = MyName;
            await hubClient.SendAnswerAsync(answer, member.Name, _roomId);
            Clients.Add(member.Name);
            OnStatusChange.Invoke();

        }

        private async Task OnIceCandidateAsync(string name, Guid peerId, RTCIceCandidateInit rTCIceCandidate)
        {
            if (rTCIceCandidate.candidate is null || rTCIceCandidate is null)
                return;

            await hubClient.SignalIceAsync(name, peerId, rTCIceCandidate, _roomId);
        }

        private async void OnJoinRoom(Guid roomId, string name, int memberCount)
        {
            MyName = name;
            _roomId = roomId;
            Connected = true;
            List<OfferRequest> offers = await rTCInterop.CreateSDPOffersAsync(memberCount, OnIceCandidateAsync);

            offers.ForEach(x => x.Name = name);
            await hubClient.SendOffersAsync(offers, name, _roomId);
            OnStatusChange.Invoke();
        }
        public async Task JoinRoomAsync(int code)
        {
            await hubClient.JoinRoomAsync(code, OnJoinRoom);
        }

        void ResetRoomInfo()
        {
            RoomCode = 0;
            _roomId = default;
            MyName = null;
            Connected = false;
            OnStatusChange.Invoke();
        }

        public async Task LeaveRoomAsync()
        {

            await hubClient.DisconnectFromRoomAsync(_roomId);
            rTCInterop.CloseConnections();
            ResetRoomInfo();
            

        }

        public void SendBuffer(byte[] buffer)
        {
            rTCInterop.SendBufferToAll(buffer);
        }

        public void SendFileChunk(byte[] buffer, string name)
        {
            rTCInterop.SendChunk(buffer, name);

        }
        public async Task<bool> SendFileMetadataAsync(FileMetadata metadata, string name)
        {
            Console.WriteLine($"sending metadata to {name}");
            rTCInterop.SendMetadata(metadata, name);

            bool messageReceived = false;
            bool ack = false;
            rTCInterop.AddListener(x =>
            {
                messageReceived = true;
                if (x == "ack")
                    ack = true;
            }, name);
            while (!messageReceived)
            {
                await Task.Delay(50);
            }
            return ack;
           

        }

        public async Task ReconnectToRoomAsync()
        {
            
            await hubClient.ReconectAsync();
            ResetRoomInfo();
        }
    }
}
