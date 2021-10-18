using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Hubs
{
    public class Room
    {
        public Room()
        {

        }

        public int Id { get; set; }
        public long masterId { get; set; }
        public string connectionMaster { get; set; }
        public long gamerId { get; set; }
        public string connectionGamer { get; set; }
        public int type { get; set; }
        public string data { get; set; }
        public bool isReady { get; set; }
        public bool isStarted { get; set; }

    }
}
