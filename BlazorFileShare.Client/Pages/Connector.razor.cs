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

        protected string offer;
        protected string answer;
        protected int code;
        protected string message;

        private async Task CreateRoomAsync()
        {
            await ClientRoomService.CreateRoomAsync(code);


        }
        private async Task JoinRoomAsync()
        {
            await ClientRoomService.JoinRoomAsync(code);

        }

        private async Task OnFileChange(InputFileChangeEventArgs args, string name)
        {
            var files = args.GetMultipleFiles();
            for (int i = 0; i < args.FileCount; i++)
            {

                var file = files[i];
                var metadata = new FileMetadata { ContentType = file.ContentType, LastModified = file.LastModified, Name = file.Name, Size = file.Size };
                var ack = await ClientRoomService.SendFileMetadataAsync(metadata, ClientRoomService.Clients[0]);
                if(ack == false)
                {
                    continue;
                }
                using var stream = file.OpenReadStream(1024 * 1024 * 1024);
                
                byte[] buffer = new byte[1024 * 16]; // read in chunks of 16KB
                int bytesRead;
                
                while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    if(bytesRead < buffer.Length)
                    {
                        buffer = new byte[bytesRead];
                        ClientRoomService.SendFileChunk(buffer, ClientRoomService.Clients[0]);
                        break;
                    }
                    ClientRoomService.SendFileChunk(buffer, ClientRoomService.Clients[0]);

                }

            }
        }

        private void SendMessage()
        {
            ClientRoomService.SendTestMessage(message);
        }

        protected override void OnInitialized()
        {
            ClientRoomService.OnStatusChange += this.StateHasChanged;
        }
    }
}
