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
                hostId = charId,
                turnId = charId
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
                    if (room.playerTwo != -1L)
                    {
                        message.data = "0|Phòng đã đủ người";
                        return message;
                    }

                    GameHub.rooms[i].playerTwo = charId;
                    GameHub.rooms[i].connectionPlayerTwo = GetConnectionId(charId);
                    GameHub.rooms[i].isReady = false;
                    GameHub.rooms[i].isStarted = false;
                    GameHub.rooms[i].turnId = GameHub.rooms[i].hostId;

                    room = GameHub.rooms[i];
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

        public async Task<Message> StartRoom(Message message)
        {
            int roomId = message.ReadInt();

            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    GameHub.rooms[i].isStarted = true;
                    GameHub.rooms[i].isEnd = false;
                    GameHub.rooms[i].data = creatDataRoom(GameHub.rooms[i].type);

                    var userOne = await _userService.UpdateCountGameAndMoneyWhenStartGame(GameHub.rooms[i].playerOne, GameHub.rooms[i].money);
                    var userTwo = await _userService.UpdateCountGameAndMoneyWhenStartGame(GameHub.rooms[i].playerTwo, GameHub.rooms[i].money);

                    message.data = GameHub.rooms[i].turnId + "|" + GameHub.rooms[i].data + "|" + userOne.money + "|" + userOne.countGame + "|" + userTwo.money + "|" + userTwo.countGame;
                    break;
                }
            }

            return message;
        }

        public async Task<Message> Attack(Message message)
        {
            int roomId = message.ReadInt();
            string data = message.ReadString();
            long turnId = message.ReadLong();

            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    GameHub.rooms[i].data = data;

                    string dameWin = getDameWin(GameHub.rooms[i].data);

                    if (dameWin == "0")
                    {
                        GameHub.rooms[i].turnId = turnId;
                        message.data = GameHub.rooms[i].Id + "|" + GameHub.rooms[i].data + "|" + turnId + "|" + dameWin;
                    }
                    else
                    {
                        User user = await _userService.UpdateCountWinAndMoneyWhenEndGame(GameHub.rooms[i].turnId, GameHub.rooms[i].money);

                        message.data = GameHub.rooms[i].Id + "|" + GameHub.rooms[i].data + "|" + turnId + "|" + dameWin + "|" + user.money + "|" + user.countWin;

                        GameHub.rooms[i].isEnd = true;
                    }

                    break;
                }
            }

            return message;
        }

        public Message SetMoney(Message message)
        {
            int roomId = message.ReadInt();
            int money = message.ReadInt();

            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].Id == roomId)
                {
                    GameHub.rooms[i].money = money;
                    message.data = money + "|" + roomId;
                    break;
                }
            }

            return message;
        }

        public Message Chat(Message message)
        {
            int roomId = message.ReadInt();
            string name = message.ReadString();
            string content = message.ReadString();

            message.data = roomId + "|[" + roomId + "] " + name + ": " + content;

            return message;
        }

        public async Task<Message> ChatGlobal(Message message)
        {
            long charId = message.ReadLong();
            string name = message.ReadString();
            string content = message.ReadString();

            await _userService.UpdateMoney(charId, -5000);

            message.data = name + ": " + content;

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

        public async Task<User> UpdateCountWinAndMoneyWhenEndGame(long id, int money)
        {
            return await _userService.UpdateCountWinAndMoneyWhenEndGame(id, money);
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

        public string getDameWin(string dataRoom)
        {
            string dataOne = isEndGame(dataRoom, "1");
            if (!dataOne.Equals("0"))
            {
                return dataOne;
            }

            string dataTwo = isEndGame(dataRoom, "2");
            if (!dataTwo.Equals("0"))
            {
                return dataTwo;
            }

            return "0";
        }

        public string isEndGame(string dataCheck, string dame)
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
                    return dame + "-0-1-2";
                }
                if (data[4].Equals(dame) && data[8].Equals(dame))
                {
                    return dame + "-0-4-8";
                }
                if (data[3].Equals(dame) && data[6].Equals(dame))
                {
                    return dame + "-0-3-6";
                }
            }
            if (data[1].Equals(dame) && data[4].Equals(dame) && data[7].Equals(dame))
            {
                return dame + "-1-4-7";
            }
            if (data[2].Equals(dame))
            {
                if (data[4].Equals(dame) && data[6].Equals(dame))
                {
                    return dame + "-2-4-6";
                }
                if (data[5].Equals(dame) && data[8].Equals(dame))
                {
                    return dame + "-2-5-8";
                }
            }
            if (data[3].Equals(dame) && data[4].Equals(dame) && data[5].Equals(dame))
            {
                return dame + "-3-4-5";
            }
            if (data[6].Equals(dame) && data[7].Equals(dame) && data[8].Equals(dame))
            {
                return dame + "-6-7-8";
            }
            return "0";
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

        public static string getStrWin(string data, int dame)
        {
            int[,] array = new int[8, 10];

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    array[i, j] = int.Parse(data.Substring(i * 8 + j, 1));
                }
            }

            return winIndexs(array, dame);
        }

        public static int getIndex(int[,] array, int i, int j)
        {
            return i * array.GetLength(1) + j;
        }

        public static string winIndexs(int[,] array, int dame)
        {
            int Rows = array.GetLength(0); // số hàng của mảng
            int Columns = array.GetLength(1); // số cột của mảng

            // check hàng ngang
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns - 5; j++)
                {
                    if (array[i, j] == dame && array[i, j + 1] == dame && array[i, j + 2] == dame && array[i, j + 3] == dame && array[i, j + 4] == dame)
                    {
                        string result = dame + "-" + getIndex(array, i, j) + "-" + getIndex(array, i, j + 1)
                            + "-" + getIndex(array, i, j + 2) + "-" + getIndex(array, i, j + 3) + "-" + getIndex(array, i, j + 4);
                        if (j == 0)
                        {
                            return result;
                        }
                        if (j == Columns - 5)
                        {
                            return result;
                        }
                        if (j != 0 && j != Columns - 5 && (array[i, j + 5] == dame || array[i, j + 5] == 0 || array[i, j - 1] == dame || array[i, j - 1] == 0))
                        {
                            return result;
                        }
                    }
                }
            }

            // check hàng dọc
            for (int j = 0; j < Columns; j++)
            {
                for (int i = 0; i < Rows - 5; i++)
                {
                    if (array[i, j] == dame && array[i + 1, j] == dame && array[i + 2, j] == dame && array[i + 3, j] == dame && array[i + 4, j] == dame)
                    {
                        string result = dame + "-" + getIndex(array, i, j) + "-" + getIndex(array, i + 1, j) + "-"
                            + getIndex(array, i + 2, j) + "-" + getIndex(array, i + 3, j) + "-" + getIndex(array, i + 4, j);
                        if (i == 0)
                        {
                            return result;
                        }
                        if (i == Rows - 5)
                        {
                            return result;
                        }
                        if (i != 0 && i != Rows - 5 && (array[i + 5, j] == dame || array[i + 5, j] == 0 || array[i - 1, j] == dame || array[i - 1, j] == 0))
                        {
                            return result;
                        }
                    }
                }
            }

            // check trái trên -> phải dưới
            for (int i = 0; i < Rows - 5; i++)
            {
                for (int j = 0; j < Columns - 5; j++)
                {
                    if (array[i, j] == dame && array[i + 1, j + 1] == dame && array[i + 2, j + 2] == dame && array[i + 3, j + 3] == dame && array[i + 4, j + 4] == dame)
                    {
                        string result = dame + "-" + getIndex(array, i, j) + "-" + getIndex(array, i + 1, j + 1) + "-"
                            + getIndex(array, i + 2, j + 2) + "-" + getIndex(array, i + 3, j + 3) + "-" + getIndex(array, i + 4, j + 4);
                        if (j == 0 || i == 0)
                        {
                            return result;
                        }
                        if (j == Columns - 5 || i == Rows - 5)
                        {
                            return result;
                        }
                        if (i != 0 && i != Rows - 5 && j != 0 && j != Columns - 5 && (array[i + 5, j + 5] == dame || array[i + 5, j + 5] == 0 || array[i - 1, j - 1] == dame || array[i - 1, j - 1] == 0))
                        {
                            return result;
                        }
                    }
                }
            }

            // check trái dưới -> phải trên
            for (int i = Rows - 1; i > 5; i--)
            {
                for (int j = 0; j < Columns - 5; j++)
                {
                    if (array[i, j] == dame && array[i - 1, j + 1] == dame && array[i - 2, j + 2] == dame && array[i - 3, j + 3] == dame && array[i - 4, j + 4] == dame)
                    {
                        string result = dame + "-" + getIndex(array, i, j) + "-" + getIndex(array, i - 1, j + 1) + "-"
                            + getIndex(array, i - 2, j + 2) + "-" + getIndex(array, i - 3, j + 3) + "-" + getIndex(array, i - 4, j + 4);
                        if (j == 0 || i == Rows - 1)
                        {
                            return result;
                        }
                        if (j == Columns - 5 || i == 4)
                        {
                            return result;
                        }
                        if (i != Rows - 1 && i != 4 && j != 0 && j != Columns - 5 && (array[i - 5, j + 5] == dame || array[i - 5, j + 5] == 0 || array[i + 1, j - 1] == dame || array[i + 1, j - 1] == 0))
                        {
                            return result;
                        }
                    }
                }
            }

            return "0";
        }
    }
}
