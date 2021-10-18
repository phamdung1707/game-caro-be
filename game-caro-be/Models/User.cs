using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Models
{
    public class User
    {
        public long id { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string toString()
        {
            return id + "|" + username;
        }
    }
}
