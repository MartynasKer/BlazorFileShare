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

        private readonly Queue<ICECandidateMessage> IceQueue = new();
        public ClientRoomService(IRTCInterop rTCInterop, IHubClient hubClient)
        {
            this.rTCInterop = rTCInterop;
            this.hubClient = hubClient;
            this.hubClient.OnAnswerReceived += OnReceiveAnswerAsync;
            this.hubClient.OnPeerJoinedRoom += OnPeerJoinedAsync;
            this.hubClient.OnInfo += Inform;
            this.hubClient.OnConnectionStateChanged += InvokeStatusChange;
            this.hubClient.OnIceCandidate += rTCInterop.AddIceAsync;
            this.rTCInterop.OnPeerLeft += (name) =>
            {
                var client = Clients.SingleOrDefault(x => x.Name == name);
                Clients.Remove(client);

                OnStatusChange?.Invoke();

            };

        }
        public List<RoomMember> Clients { get; set; } = new List<RoomMember>();
        public RoomMember MyMember { get; private set; }

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
            RoomCode = code;
        }


        private void OnRTCConnected(RoomMember member)
        {
            Clients.Add(member);
            OnStatusChange.Invoke();
        }

        private async Task OnReceiveAnswerAsync(AnswerRequest request)
        {
            Console.WriteLine("received answer...");
            await rTCInterop.SetSDPAnswerAsync(request, request.Name, () => OnRTCConnected(request.RoomMember));
            OnStatusChange.Invoke();
        }

        public void SendTestMessage(string message)
        {
            rTCInterop.SendMessageToAll(message);
        }

        private async Task OnPeerJoinedAsync(RoomMember member)
        {
            Console.WriteLine("peer joined");
            await rTCInterop.SetSDPOfferAsync(member.Offer, member.Name, OnIceCandidateAsync,()=> OnRTCConnected(member));
            var answer = await rTCInterop.CreateSDPAnswerAsync(member.Name, member.Offer.PeerConnectionId);
            answer.RoomMember = MyMember;
            await hubClient.SendAnswerAsync(answer, member.Name, _roomId);
            OnStatusChange.Invoke();

        }

        private async Task OnIceCandidateAsync(string name, Guid peerId, RTCIceCandidateInit rTCIceCandidate)
        {
            if (rTCIceCandidate.candidate is null || rTCIceCandidate is null)
                return;

            await hubClient.SignalIceAsync(name, peerId, rTCIceCandidate, _roomId);
        }

        private async void OnJoinRoom(Guid roomId, RoomMember member, int memberCount)
        {
            
            MyMember = member;
            _roomId = roomId;
            Connected = true;
            List<OfferRequest> offers = await rTCInterop.CreateSDPOffersAsync(memberCount, OnIceCandidateAsync);

            offers.ForEach(x => x.Name = member.Name);
            await hubClient.SendOffersAsync(offers, member.Name, _roomId);
            OnStatusChange.Invoke();
        }
        public async Task JoinRoomAsync(int code)
        {
            await hubClient.JoinRoomAsync(code, OnJoinRoom);
            RoomCode = code;
        }

        void ResetRoomInfo()
        {
            RoomCode = 0;
            _roomId = default;
            MyMember = null;
            Connected = false;
            Clients.Clear();
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

        public async Task SendFileChunkAsync(byte[] buffer, string name, int chunk_number)
        {
            await rTCInterop.SendChunkAsync(buffer, name, chunk_number);
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
