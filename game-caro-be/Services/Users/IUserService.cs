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
    }
}
