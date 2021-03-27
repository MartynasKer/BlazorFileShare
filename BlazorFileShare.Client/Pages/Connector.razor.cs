﻿using Blazored.Modal.Services;
using BlazorFileShare.Client.Domain;
using BlazorFileShare.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Pages
{
    public partial class Connector : ComponentBase
    { 

        [Inject]
        private IClientRoomService ClientRoomService { get; set; }

        private Dictionary<string, InputFile> fileInputs = new();
        protected string offer;
        protected string answer;
        int code;
        protected int Code {
            get
            {
                return code;
            }
            set
            {
                if(value < 10000)
                {
                    code = value;
                    this.StateHasChanged();
                }
            }
        
        }

        bool CanConnect
        {
            get
            {
                if(State != UIState.Idle && State != UIState.Connected)
                {
                    return Code <= 999;
                }
                return false;
            }

        }
        protected string message;
        protected UIState State = UIState.Idle; 
        

        private async Task CreateRoomAsync()
        {

            if (State == UIState.Idle)
            {
                State = UIState.Creating;
                return;
            }
            if (State == UIState.Creating)
            {
                await ClientRoomService.CreateRoomAsync(code);
            }

        }
        private async Task JoinRoomAsync()
        {
            if (State == UIState.Idle)
            {
                State = UIState.Joining;
                return;
            }
            if (State == UIState.Joining)
            {
                await ClientRoomService.JoinRoomAsync(code);
            }
        }
        private void OnFileChangeAll(InputFileChangeEventArgs args)
        {


            foreach(var client in ClientRoomService.Clients)
            {
                OnFileChange(args, client.Name);
            }


        }
        private async Task OnFileChange(InputFileChangeEventArgs args, string name)
        {
            
            var files = args.GetMultipleFiles();
            for (int i = 0; i < args.FileCount; i++)
            {
                
                var file = files[i];
                var metadata = new FileMetadata { ContentType = file.ContentType, LastModified = file.LastModified, Name = file.Name, Size = file.Size };
                var ack = await ClientRoomService.SendFileMetadataAsync(metadata, name);
                if(ack == false)
                {
                    continue;
                }
                
                using var stream = file.OpenReadStream(1024*1024*1024);//max 1 GB
                
                byte[] buffer = new byte[1024 * 16]; // read in chunks of 16KB
                int bytesRead;
                
                while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    if(bytesRead < buffer.Length)
                    {
                        buffer = new byte[bytesRead];
                        ClientRoomService.SendFileChunk(buffer, name);
                        break;
                    }
                    ClientRoomService.SendFileChunk(buffer, name);

                }

            }
        }

        private async Task ReconnectAsync()
        {
            await ClientRoomService.ReconnectToRoomAsync();

        }
        private async Task LeaveRoomAsync()
        {
            await ClientRoomService.LeaveRoomAsync();
        }

        private void SendMessage()
        {
            ClientRoomService.SendTestMessage(message);
            message = null;
        }

        protected override void OnInitialized()
        {
            ClientRoomService.OnStatusChange += this.StateHasChanged;
        }
    }
    public enum UIState
    {
        Idle,
        Joining,
        Creating,
        Connected

    }
}
