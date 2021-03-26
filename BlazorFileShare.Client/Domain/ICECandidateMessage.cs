using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Domain
{
    public class ICECandidateMessage
    {
        public string Name { get; set; }

        public Guid PeerId { get; set; }

        public RTCIceCandidateInit rtcIceCandidate { get; set; }




    }
}
