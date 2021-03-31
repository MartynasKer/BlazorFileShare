using BlazorFileShare.Client.Domain;
using BlazorFileShare.Domain;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Services
{
    public interface IClientRoomService
    {
        event Action OnStatusChange;
        RoomMember MyMember { get; }
        int RoomCode { get; }
        bool Connected { get; }

        List<RoomMember> Clients { get; }
        HubConnectionState State { get; }
        Task JoinRoomAsync(int code);
        Task CreateRoomAsync(int code);
        Task LeaveRoomAsync();
        void SendTestMessage(string message);
        void SendBuffer(byte[] buffer);
        Task SendFileChunkAsync(byte[] buffer, string name);
        Task<bool> SendFileMetadataAsync(FileMetadata metadata, string name);
        Task ReconnectToRoomAsync();
    }
}
