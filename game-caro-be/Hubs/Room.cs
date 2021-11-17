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
        public long playerOne { get; set; }
        public string connectionPlayerOne { get; set; }
        public long playerTwo { get; set; }
        public string connectionPlayerTwo { get; set; }
        public int type { get; set; }
        public string data { get; set; }
        public bool isReady { get; set; }
        public bool isStarted { get; set; }
        public int money { get; set; }

        public long hostId { get; set; }

    }
}
