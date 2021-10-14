using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Hubs
{
    public class GameHub : Hub
    {
        public async Task SendString(string text)
        {
            Console.WriteLine(text);
            await Clients.All.SendAsync("receiveMessage", text);
        }
    }

}
