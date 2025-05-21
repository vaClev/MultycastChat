using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClientChatMultcast
{
    public class User
    {
        public string name { get; set; }
        public string ip { get; set; }

        [JsonConstructor]
        public User (string name, string ip)
        {
            this.name = name;
            this.ip = ip;
        }

    }
}
