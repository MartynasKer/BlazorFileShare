using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorFileShare.Shared.Domain
{
    public class OfferRequest
    {
        public OfferRequest()
        {
        }

        public OfferRequest(string name, string offer, Guid peerId)
        {
            Name = name;
            Offer = offer;
            PeerConnectionId = peerId;

        }

        public Guid PeerConnectionId { get; set; }
        public string Name { get; set; }

        public string Offer { get; set; }





    }
}
