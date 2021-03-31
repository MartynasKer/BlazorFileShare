using Blazor.DownloadFileFast.Interfaces;
using Blazored.Modal;
using Blazored.Modal.Services;
using BlazorFileShare.Client.Domain;
using BlazorFileShare.Client.Shared;
using BlazorFileShare.Shared;
using BlazorFileShare.Shared.Domain;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Services
{
    public class RTCInterop : IRTCInterop
    {
        private readonly List<Peer> ClientList = new();
        private readonly IModalService modalService;

        private readonly IJSRuntime jSRuntime;
        private readonly IBlazorDownloadFileService blazorDownloadFileService;

        public event Action<string> OnPeerLeft;

        public event Action<Message> OnMessage;

        public RTCInterop(IJSRuntime jSRuntime, IModalService modalService, IBlazorDownloadFileService blazorDownloadFileService)
        {
            this.jSRuntime = jSRuntime;
            this.modalService = modalService;
            this.blazorDownloadFileService = blazorDownloadFileService;
        }

        public async Task<AnswerRequest> CreateSDPAnswerAsync(string name, Guid peerId)
        {
            var peer = ClientList.SingleOrDefault(x => x.PeerConnectionId == peerId);
            if (peer is null) return null;

            var answer = await peer.RTCPeerConnection.createAnswer();
            await peer.RTCPeerConnection.setLocalDescription(answer);
            return new AnswerRequest(null, answer.sdp, peerId);
        }

        private void RemovePeer(Peer peer)
        {
            ClientList.Remove(peer);
            Console.WriteLine($"peer {peer.Name} connection closed");
            OnPeerLeft?.Invoke(peer.Name);
        }
        public async Task<List<OfferRequest>> CreateSDPOffersAsync(int memberCount, Func<string, Guid, RTCIceCandidateInit, Task> OnIceCandidate)
        {
            List<OfferRequest> list = new();
            for (int i = 0; i < memberCount - 1; i++)
            {

                var peer = new Peer(null, default, OnIceCandidate, DownloadFile, RemovePeer, OnMessage.Invoke);
                peer.OnFileMetadata += OnMetadataAsync;
                peer.CreateDataChannel(i.ToString());
                var result = await peer.RTCPeerConnection.createOffer();
                var offer = result.sdp;
                await peer.RTCPeerConnection.setLocalDescription(result);
                var offerRequest = new OfferRequest { Offer = offer, PeerConnectionId = peer.PeerConnectionId };
                ClientList.Add(peer);
                list.Add(offerRequest);
            }
            return list;
        }

        async void OnMetadataAsync(Peer peer, FileMetadata metadata)
        {
            var parameters = new ModalParameters();
            parameters.Add("Name", peer.Name);
            parameters.Add("FileSize", metadata.Size);
            parameters.Add("FileName", metadata.Name);
            var modal = modalService.Show<MetadataModal>("File donwload request",parameters);
            var result = await modal.Result;
            if(result.Cancelled == false)
            {
                peer.SendMessage("ack");
            }
            else
            {
                peer.SendMessage("cancel");
            }
        }

        async void DownloadFile(Peer peer)
        {

            await blazorDownloadFileService.DownloadFileAsync(peer.CurrentFileMetadata.Name, peer.FileBuffer.SelectMany(a=>a).ToArray());

        }
        public async Task SetSDPAnswerAsync(AnswerRequest answerRequest, string name, Action onRTCConnected)
        {
            var peer = ClientList.SingleOrDefault(x => x.PeerConnectionId == answerRequest.PeerConnectionId);
            var answerSDP = new RTCSessionDescriptionInit(answerRequest.Answer, RTCSdpType.Answer);
            peer.RTCDataChannel.OnOpen += (s,e) => onRTCConnected.Invoke();
            peer.Name = name;
            await peer.RTCPeerConnection.setRemoteDescription(answerSDP);
            await peer.TrickleIceAsync();
        }




        public async Task SetSDPOfferAsync(OfferRequest offer, string name, Func<string, Guid, RTCIceCandidateInit, Task> OnIceCandidate, Action onRTCConnected)
        {
            var peer = new Peer(name, offer.PeerConnectionId, OnIceCandidate, DownloadFile, RemovePeer, OnMessage.Invoke, onRTCConnected);
            peer.OnFileMetadata += OnMetadataAsync;
            var offerSDP = new RTCSessionDescriptionInit(offer.Offer, RTCSdpType.Offer);
            await peer.RTCPeerConnection.setRemoteDescription(offerSDP);
            ClientList.Add(peer);
            Console.WriteLine($"received offer {offer.PeerConnectionId}");

        }

        public async Task AddIceAsync(RTCIceCandidate arg1, Guid arg2)
        {
            var peer = ClientList.SingleOrDefault(x => x.PeerConnectionId == arg2);
            if (peer != null)
            {
                await peer.RTCPeerConnection.addIceCandidate(arg1);
            }
        }

        public void SendMessageToAll(string message)
        {
            foreach (var peer in ClientList)
            {
                if (peer.RTCDataChannel.ReadyState == RTCDataChannelState.Open)
                {
                    peer.RTCDataChannel.Send(message);

                }
            }
        }


        public void AddListener(Action<string> action, string name)
        {
            ClientList.SingleOrDefault(x => x.Name == name).AddMessageListener(action);
        }
        public void CloseConnections()
        {
            ClientList.ForEach(x => x.RTCPeerConnection.close());
            ClientList.Clear();
        }

        public void SendBufferToAll(byte[] buffer)
        {
            foreach (var peer in ClientList)
            {
                if (peer.RTCDataChannel.ReadyState == RTCDataChannelState.Open)
                {
                    peer.RTCDataChannel.Send(buffer);

                }
            }
        }

        public async Task SendChunkAsync(byte[] buffer, string name)
        {
            await ClientList.SingleOrDefault(x => x.Name == name).SendFileChunkAsync(buffer);
        }

        public void SendMetadata(FileMetadata metadata, string name)
        {
            ClientList.SingleOrDefault(x => x.Name == name).SendFileMetadata(metadata);

        }
    }
}
