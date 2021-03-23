using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.WebRTC
{
    public class RTCPeerConfiguration
    {
       
        public List<IceServer> iceServers { get; set; }

    }
    public class IceServer
    {
        public string urls { get; set; }

    }
}
