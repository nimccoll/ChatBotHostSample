using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QnABotHost.Models
{
    public class DirectLineToken
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
        public string streamUrl { get; set; }
    }
}
