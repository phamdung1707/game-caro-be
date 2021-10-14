using game_caro_be.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Hubs
{
    public class GameHub : Hub
    {
        public static Dictionary<string, long> userLogins = new Dictionary<string, long>();

        private readonly IGameService _gameService;

        public GameHub(IGameService gameService)
        {
            _gameService = gameService;
        }

        public override Task OnConnectedAsync()
        {
            userLogins.Add(Context.ConnectionId, -1L);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            userLogins.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string messageId, string data)
        {
            int id = -1;        
            try
            {
                id = int.Parse(messageId);
            }
            catch
            {
            }
            Message message = new Message(id, data);
            Message resultMessage = null;
            switch (id)
            {
                case 0:
                    resultMessage = await _gameService.Login(message);

                    if (resultMessage.data.StartsWith("1"))
                    {
                        userLogins[Context.ConnectionId] = long.Parse(resultMessage.data.Split('|')[1]);
                    }

                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);

                    break;
                case 1:
                    resultMessage = await _gameService.Login(message);          

                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
            }


        }
    }

}
