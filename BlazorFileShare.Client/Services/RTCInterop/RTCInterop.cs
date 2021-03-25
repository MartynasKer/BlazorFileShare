using BlazorFileShare.Client.Domain;
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

        private readonly IJSRuntime jSRuntime;

        public event Action<string> OnPeerLeft;
        public RTCInterop(IJSRuntime jSRuntime)
        {
            this.jSRuntime = jSRuntime;
        }

        public async Task<AnswerRequest> CreateSDPAnswerAsync(string name, Guid peerId)
        {
            var peer = ClientList.SingleOrDefault(x => x.PeerConnectionId == peerId);
            if (peer is null) return null;

            var answer = await peer.RTCPeerConnection.createAnswer();
            await peer.RTCPeerConnection.setLocalDescription(answer);
            return new AnswerRequest(name, answer.sdp, peerId);
        }

        private void RemovePeer(Peer peer)
        {
            ClientList.Remove(peer);
            OnPeerLeft?.Invoke(peer.Name);
        }
        public async Task<List<OfferRequest>> CreateSDPOffersAsync(int memberCount, Func<string, Guid, RTCIceCandidateInit, Task> OnIceCandidate)
        {
            List<OfferRequest> list = new();
            for (int i = 0; i < memberCount - 1; i++)
            {

                var peer = new Peer(null, default, OnIceCandidate, DownloadFile, RemovePeer);
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

        void DownloadFile(Peer peer)
        {

            var task = jSRuntime.InvokeVoidAsync("downloadFromByteArray",
                new
                {
                    ByteArray = peer.FileBuffer.SelectMany(a => a).ToArray(),
                    FileName = peer.CurrentFileMetadata.Name,
                    ContentType = peer.CurrentFileMetadata.ContentType
                });

        }
        public async Task SetSDPAnswerAsync(AnswerRequest answerRequest, string name, Action<string> onRTCConnected)
        {
            var peer = ClientList.SingleOrDefault(x => x.PeerConnectionId == answerRequest.PeerConnectionId);
            var answerSDP = new RTCSessionDescriptionInit(answerRequest.Answer, RTCSdpType.Answer);
            peer.Name = name;
            await peer.RTCPeerConnection.setRemoteDescription(answerSDP);

        }




        public async Task SetSDPOfferAsync(OfferRequest offer, string name, Func<string, Guid, RTCIceCandidateInit, Task> OnIceCandidate, Action<string> onRTCConnected)
        {
            var peer = new Peer(name, offer.PeerConnectionId, OnIceCandidate, DownloadFile, RemovePeer);

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

        public void SendChunk(byte[] buffer, string name)
        {
            ClientList.SingleOrDefault(x => x.Name == name).SendFileChunk(buffer);
        }

        public void SendMetadata(FileMetadata metadata, string name)
        {
            ClientList.SingleOrDefault(x => x.Name == name).SendFileMetadata(metadata);

        }
    }
}
