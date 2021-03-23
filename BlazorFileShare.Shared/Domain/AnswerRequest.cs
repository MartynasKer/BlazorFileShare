using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Shared
{
    public class AnswerRequest
    {

        public AnswerRequest()
        {

        }
        public AnswerRequest(string name, string answer, Guid peerId)
        {
            Name = name;
            Answer = answer;
            PeerConnectionId = peerId;

        }

        public Guid PeerConnectionId { get; set; }
        public string Name { get; set; }

     

        public string Answer { get; set; }
    }
}
