using game_caro_be.Hubs;
using game_caro_be.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Services
{
    public class GameService : IGameService
    {
        private readonly IUserService _userService;

        public GameService(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Message> Login(Message message)
        {
            string username = message.ReadString();
            string password = message.ReadString();

            message.data = await _userService.Login(username, password);

            return message;
        }

        public async Task<Message> Register(Message message)
        {
            string username = message.ReadString();
            string password = message.ReadString();

            message.data = await _userService.Register(username, password);

            return message;
        }
    }
}
