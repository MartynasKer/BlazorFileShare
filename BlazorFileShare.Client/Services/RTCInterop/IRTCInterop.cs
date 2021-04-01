using BlazorFileShare.Client.Domain;
using BlazorFileShare.Shared;
using BlazorFileShare.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Services
{
    public interface IRTCInterop
    {
        event Action<string> OnPeerLeft;

        event Action<Message> OnMessage;
        event Action<float> OnReportProgress;

        Task<AnswerRequest> CreateSDPAnswerAsync(string name, Guid peerId);
        Task SetSDPAnswerAsync(AnswerRequest answer, string name, Action onRTCConnected);
        Task<List<OfferRequest>> CreateSDPOffersAsync(int memberCount, Func<string, Guid, RTCIceCandidateInit, Task> OnIceCandidate);
        Task AddIceAsync(RTCIceCandidate arg1, Guid arg2);
        Task SetSDPOfferAsync(OfferRequest offer, string name, Func<string, Guid, RTCIceCandidateInit, Task> OnIceCandidate, Action onRTCConnected);
        void SendMessageToAll(string message);
        void SendBufferToAll(byte[] buffer);
        Task SendChunkAsync(byte[] buffer, string name, int chunk_number);
        void SendMetadata(FileMetadata metadata, string name);
        void CloseConnections();
        void AddListener(Action<string> action, string name);
    }
}
