using BlazorFileShare.Client.Domain;
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
        string MyName { get; }
        int RoomCode { get; }
        bool Connected { get; }

        List<string> Clients { get; }
        HubConnectionState State { get; }
        Task JoinRoomAsync(int code);
        Task CreateRoomAsync(int code);
        Task LeaveRoomAsync();
        void SendTestMessage(string message);
        void SendBuffer(byte[] buffer);
        void SendFileChunk(byte[] buffer, string name);
        Task<bool> SendFileMetadataAsync(FileMetadata metadata, string name);
      
    }
}
