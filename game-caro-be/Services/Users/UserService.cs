using game_caro_be.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Services.Users
{
    public class UserService : IUserService
    {
        public readonly GameDbContext _context;
        public UserService(GameDbContext context)
        {
            _context = context;
        }

        public async Task<string> Register(string username, string password)
        {
            var users = await _context.Users.Where(us => us.username == username).ToListAsync();

            if (users.Count > 0)
            {
                return "0|Tài khoản đã tồn tại";
            }

            User user = new User()
            {
                username = username,
                password = password,
                money = 100000L,
                countGame = 0,
                countWin = 0
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return "1|Đăng ký thành công tài khoản \"" + username + "\" mật khẩu \"" + password + "\"";
        }

        public async Task<string> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(us => us.username == username && us.password == password);

            if (user == null)
            {
                return "0|Thông tin tài khoản hoặc mật khẩu không chính xác";
            }

            return "1|" + user.id + "|" + user.username + "|" + user.money + "|" + user.countWin + "|" + user.countGame;
        }

        public async Task<User> FindById(long id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> UpdateMoney(long id, int money)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return user;
            }

            user.money += money;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task UpdateCountGame(long id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return;
            }

            user.countGame ++;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return;
        }

        public async Task<User> UpdateCountGameAndMoneyWhenStartGame(long id, int money)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            user.countGame++;

            user.money -= money;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateCountWinAndMoneyWhenEndGame(long id, int money)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            user.countWin++;

            user.money += money;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task UpdateCountWin(long id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return;
            }

            user.countWin++;

            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return;
        }
    }
}
