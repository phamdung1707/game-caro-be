using game_caro_be.Hubs;
using game_caro_be.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Services
{
    public interface IGameService
    {
        Task<Message> Login(Message message);

        Task<Message> Register(Message message);

        Message CreateRoom(Message message);

        void cleanRoom(long charId);

        Message JoinRoom(Message message);

        Task<Message> GetUserById(long id);

        Message ReadyRoom(Message message);

        Task<Message> StartRoom(Message message);

        Task<Message> Attack(Message message);

        Task<User> UpdateCountWinAndMoneyWhenEndGame(long id, int money);

        Message SetMoney(Message message);

        Message Chat(Message message);

        Task<Message> ChatGlobal(Message message);
    }
}
