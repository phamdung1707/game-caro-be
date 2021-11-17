using game_caro_be.Hubs;
using game_caro_be.Models;
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

        public static Random random = new Random();

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

        public Message CreateRoom(Message message)
        {
            int money = message.ReadInt();
            int type = message.ReadInt();
            long charId = message.ReadLong();

            cleanRoom(charId);

            Room room = new Room()
            {
                playerTwo = -1L,
                playerOne = charId,
                connectionPlayerOne = GetConnectionId(charId),
                isReady = false,
                type = type,
                isStarted = false,
                data = creatDataRoom(type),
                money = money,
                hostId = charId
            };

            room.Id = random.Next(100000, 999999);

            while (isExistRoom(room.Id))
            {
                room.Id = random.Next(100000, 999999);
            }

            GameHub.rooms.Add(room);

            message.data = room.Id + "|" + room.hostId + "|" + room.money + "|" + room.type + "|" + room.data;

            Console.WriteLine(message.data);

            return message;
        }

        public Message JoinRoom(Message message)
        {
            int roomId = message.ReadInt();

            long charId = message.ReadLong();

            Console.WriteLine(roomId);

            if (!isExistRoom(roomId))
            {
                message.data = "0|Không tìm thấy phòng phù hợp!";
                return message;
            }

            Room room = null;

            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    room = GameHub.rooms[i];
                    break;
                }
            }

            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    room = GameHub.rooms[i];
                    if (room.playerTwo != -1L)
                    {
                        message.data = "0|Phòng đã đủ người";
                        return message;
                    }
                    GameHub.rooms[i].playerTwo = charId;
                    GameHub.rooms[i].connectionPlayerTwo = GetConnectionId(charId);
                    break;
                }
            }

            message.data = "1|" + room.Id + "|" + room.hostId + "|" + room.money + "|" + room.type + "|" + room.data;

            return message;
        }

        public async Task<Message> GetUserById(long id)
        {
            User user = await _userService.FindById(id);

            string data = "1|" + user.id + "|" + user.username + "|" + user.money + "|" + user.countWin + "|" + user.countGame;

            Message message = new Message(5, data);

            return message;
        }

        public Message ReadyRoom(Message message)
        {
            int roomId = message.ReadInt();

            if (!isExistRoom(roomId))
            {
                message.data = "0|Không tìm thấy phòng phù hợp!";
                return message;
            }

            Room room = null;

            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    room = GameHub.rooms[i];
                    break;
                }
            }

            room.isReady = true;

            message.data = "1|" + room.Id;

            return message;
        }

        public Message StartRoom(Message message)
        {
            int roomId = message.ReadInt();

            if (!isExistRoom(roomId))
            {
                message.data = "0|Không tìm thấy phòng phù hợp!";
                return message;
            }

            Room room = null;

            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    room = GameHub.rooms[i];
                    break;
                }
            }

            if (!room.isReady)
            {
                message.data = "0|Đối thủ chưa sẵn sàng!";
                return message;
            }

            room.isStarted = true;
            room.data = "000000000";

            message.data = "1|" + room.Id;

            return message;
        }

        public Message Attack(Message message)
        {
            int roomId = message.ReadInt();

            if (!isExistRoom(roomId))
            {
                message.data = "0|Không tìm thấy phòng phù hợp!";
                return message;
            }

            Room room = null;

            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    room = GameHub.rooms[i];
                    break;
                }
            }

            if (!room.isStarted)
            {
                message.data = "0|Trận đấu chưa bắt đầu!";
                return message;
            }

            int indexAttack = message.ReadInt();
            int indexReplace = message.ReadInt();
            string textAttack = message.ReadString();

            string currentData = "";

            for (int i =0; i < room.data.Length; i++)
            {
                if (i != indexAttack)
                {
                    if (indexReplace != -1 && i == indexReplace)
                    {
                        currentData += "0";
                    }
                    else
                    {
                        currentData += room.data[i];
                    }                   
                }
                else
                {
                    currentData += textAttack;
                }               
            }

            room.data = currentData;

            message.data = "1|" + room.Id + "|" + currentData + "|" + (isEndGame(currentData, textAttack) ? "1" : "0");

            return message;
        }

        public void cleanRoom(long charId)
        {
            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].playerOne == charId)
                {
                    GameHub.rooms.RemoveAt(i);
                    i--;
                }
                else if (GameHub.rooms[i].playerTwo == charId)
                {
                    GameHub.rooms[i].playerTwo = -1L;
                    GameHub.rooms[i].connectionPlayerTwo = "";
                }
            }
        }

        public bool isExistRoom(int roomId)
        {
            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    return true;
                }
            }
            return false;
        }

        public Room findRoomById(int roomId)
        {
            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    return GameHub.rooms[i];
                }
            }
            return null;
        }

        public string GetConnectionId(long charId)
        {
            foreach (var item in GameHub.userLogins)
            {
                if (item.Value == charId)
                {
                    return item.Key;
                }
            }
            return "";
        }

        public bool isEndGame(string dataCheck, string dame)
        {
            string[] data = new string[dataCheck.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = dataCheck[i].ToString();
            }

            if (data[0].Equals(dame))
            {
                if (data[1].Equals(dame) && data[2].Equals(dame))
                {
                    return true;
                }
                if (data[4].Equals(dame) && data[8].Equals(dame))
                {
                    return true;
                }
                if (data[3].Equals(dame) && data[6].Equals(dame))
                {
                    return true;
                }
            }
            if (data[1].Equals(dame) && data[4].Equals(dame) && data[7].Equals(dame))
            {
                return true;
            }
            if (data[2].Equals(dame))
            {
                if (data[4].Equals(dame) && data[6].Equals(dame))
                {
                    return true;
                }
                if (data[5].Equals(dame) && data[8].Equals(dame))
                {
                    return true;
                }
            }
            if (data[3].Equals(dame) && data[4].Equals(dame) && data[5].Equals(dame))
            {
                return true;
            }
            if (data[6].Equals(dame) && data[7].Equals(dame) && data[8].Equals(dame))
            {
                return true;
            }
            return false;
        }

        public string creatDataRoom(int type)
        {
            if (type == 0)
            {
                return "000000000";
            }

            string data = "";
            for (int i = 0; i < 100; i++)
            {
                data += "0";
            }

            return data;
        }
    }
}
