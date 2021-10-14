using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Hubs
{
    public class Message
    {
        public int messageId { get; set; }
        public string data { get; set; }

        public Message(int messageId, string data)
        {
            this.messageId = messageId;
            this.data = data;
        }

        public string ReadString()
        {
            if (data.StartsWith("|"))
            {
                data = data.Substring(1);
            }

            if (!data.Contains("|"))
            {
                return data;
            }

            string result = data.Split('|')[0];
            data = data.Substring(result.Length);

            return result;
        }
    }
}
