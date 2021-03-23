using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Services
{
    public class RTCPeerClient
    {
        private readonly RTCPeerConnection peerConnection;
        private readonly RTCDataChannel dataChannel;
        public RTCPeerClient(RTCPeerConnection peerConnection)
        {
            this.peerConnection = peerConnection;
            dataChannel = peerConnection.createDataChannel(Guid.NewGuid().ToString());
        }

        public string Name { get; set; }

        public async Task CreateSDPOfferAsync()
        {
            await peerConnection.createOffer();
        }

    }
}
