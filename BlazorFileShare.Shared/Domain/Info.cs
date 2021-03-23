using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorFileShare.Shared.Domain
{
    public class Info
    {
        public Info(string message, bool success)
        {
            Message = message;
            Success = success;
        }

        public string Message { get; set; }

        public bool Success { get; set; }


    }
}
