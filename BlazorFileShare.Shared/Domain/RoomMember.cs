using BlazorFileShare.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebRTC;
using System.Threading.Tasks;

namespace BlazorFileShare.Domain
{
    public class RoomMember
    {
        public OfferRequest Offer { get; set; }

        public string Name { get; set; }

        public string ConnectionId { get; set; }
    }
}
