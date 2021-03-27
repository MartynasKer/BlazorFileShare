using BlazorFileShare.Domain;
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
        public AnswerRequest(RoomMember member, string answer, Guid peerId)
        {
            RoomMember = member;
            Answer = answer;
            PeerConnectionId = peerId;

        }

        public Guid PeerConnectionId { get; set; }
        public string Name { get { return RoomMember.Name; }}

        public RoomMember RoomMember { get; set; }

        public string Answer { get; set; }
    }
}
