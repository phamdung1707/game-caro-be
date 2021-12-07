using game_caro_be.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Services.Users
{
    public interface IUserService
    {
        Task<string> Register(string username, string password);
        Task<string> Login(string username, string password);
        Task<User> FindById(long id);
        Task<User> UpdateMoney(long id, int money);
        Task UpdateCountWin(long id);
        Task UpdateCountGame(long id);
        Task<User> UpdateCountGameAndMoneyWhenStartGame(long id, int money);
        Task<User> UpdateCountWinAndMoneyWhenEndGame(long id, int money);
    }
}
