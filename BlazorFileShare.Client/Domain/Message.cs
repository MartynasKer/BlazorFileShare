using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFileShare.Client.Domain
{
    public class Message
    {
        public Message(string message, string name)
        {
            Data = message;
            Name = name;
        }

        public string Data { get; set; }

        public string Name { get; set; }

    }
}
