﻿using BlazorFileShare.Client.Domain;
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
        Task<AnswerRequest> CreateSDPAnswerAsync(string name, Guid peerId);
        Task SetSDPAnswerAsync(AnswerRequest answer, string name, Action<string> onRTCConnected);
        Task<List<OfferRequest>> CreateSDPOffersAsync(int memberCount, Func<string, Guid, RTCIceCandidateInit, Task> OnIceCandidate);
        Task AddIceAsync(RTCIceCandidate arg1, Guid arg2);
        Task SetSDPOfferAsync(OfferRequest offer, string name, Func<string, Guid, RTCIceCandidateInit, Task> OnIceCandidate, Action<string> onRTCConnected);
        void SendMessageToAll(string message);
        void SendBufferToAll(byte[] buffer);
        void SendChunk(byte[] buffer, string name);
        void SendMetadata(FileMetadata metadata, string name);
        void CloseConnections();
    }
}