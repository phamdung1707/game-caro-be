using game_caro_be.Models;
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

        public static List<Room> rooms = new List<Room>();

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
            //_gameService.cleanRoom(userLogins[Context.ConnectionId]);
            try
            {
                for (int i = 0; i < GameHub.rooms.Count; i++)
                {
                    if (GameHub.rooms[i].playerOne == userLogins[Context.ConnectionId])
                    {
                        if (GameHub.rooms[i].playerTwo != -1)
                        {
                            Message message = new Message(5, "0");
                            if (GameHub.rooms[i].isStarted)
                            {
                                UpdateCountWinAndMoneyWhenEndGame(GameHub.rooms[i].connectionPlayerTwo, message, GameHub.rooms[i].playerTwo, GameHub.rooms[i].money).Wait();
                            }
                            else
                            {
                                SendMsg(GameHub.rooms[i].connectionPlayerTwo, message).Wait();
                            }

                        }

                        GameHub.rooms.RemoveAt(i);
                        i--;
                    }
                    else if (GameHub.rooms[i].playerTwo == userLogins[Context.ConnectionId])
                    {
                        Message message = new Message(5, "0");

                        if (GameHub.rooms[i].isStarted)
                        {
                            UpdateCountWinAndMoneyWhenEndGame(GameHub.rooms[i].connectionPlayerOne, message, GameHub.rooms[i].playerOne, GameHub.rooms[i].money).Wait();
                        }
                        else
                        {
                            SendMsg(GameHub.rooms[i].connectionPlayerOne, message).Wait();
                        }



                        GameHub.rooms[i].playerTwo = -1L;
                        GameHub.rooms[i].connectionPlayerTwo = "";
                    }
                }
            }
            catch
            {
            }
           

            userLogins.Remove(Context.ConnectionId);        
            return base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateCountWinAndMoneyWhenEndGame(string connectionId, Message message, long id, int money)
        {
            if (id < 1)
            {
                return;
            }

            User user = await _gameService.UpdateCountWinAndMoneyWhenEndGame(id, money);

            message.data = "0|" + user.money + "|" + user.countWin;

            await Clients.Clients(connectionId).SendAsync("ReceiveMessage", message.messageId.ToString(), message.data);
        }

        public async Task SendMsg(string connectionId, Message message)
        {
            await Clients.Clients(connectionId).SendAsync("ReceiveMessage", message.messageId.ToString(), message.data);
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

                    // set userId to connectionId
                    if (resultMessage.data.StartsWith("1"))
                    {
                        userLogins[Context.ConnectionId] = long.Parse(resultMessage.data.Split('|')[1]);
                    }

                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);

                    break;
                case 1:
                    resultMessage = await _gameService.Register(message);          

                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
                case 2:
                    resultMessage = message;

                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
                case 3:                 
                    resultMessage = _gameService.CreateRoom(message);
                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
                case 4:
                    resultMessage = _gameService.JoinRoom(message);

                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);

                    if (resultMessage.data.StartsWith("1"))
                    {
                        int roomJoinId = int.Parse(resultMessage.data.Split('|')[1]);

                        Room roomJoin = GetRoomById(roomJoinId);

                        resultMessage = await _gameService.GetUserById(roomJoin.playerOne);
                        await Clients.Clients(roomJoin.connectionPlayerTwo).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);

                        resultMessage = await _gameService.GetUserById(roomJoin.playerTwo);
                        await Clients.Clients(roomJoin.connectionPlayerOne).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    }               
                    break;
                case 6:
                    for (int i = 0; i < GameHub.rooms.Count; i++)
                    {
                        if (GameHub.rooms[i].playerOne == userLogins[Context.ConnectionId])
                        {
                            if (GameHub.rooms[i].playerTwo != -1)
                            {
                                resultMessage = new Message(5, "0");
                                if (GameHub.rooms[i].isStarted)
                                {
                                    User user = await _gameService.UpdateCountWinAndMoneyWhenEndGame(GameHub.rooms[i].playerTwo, GameHub.rooms[i].money);
                                    resultMessage.data = "0|" + user.money + "|" + user.countWin;
                                }

                                SendMsg(GameHub.rooms[i].connectionPlayerTwo, resultMessage).Wait();
                            }

                            GameHub.rooms.RemoveAt(i);
                            i--;
                        }
                        else if (GameHub.rooms[i].playerTwo == userLogins[Context.ConnectionId])
                        {
                            resultMessage = new Message(5, "0");
                            if (GameHub.rooms[i].isStarted)
                            {
                                User user = await _gameService.UpdateCountWinAndMoneyWhenEndGame(GameHub.rooms[i].playerOne, GameHub.rooms[i].money);
                                resultMessage.data = "0|" + user.money + "|" + user.countWin;
                            }

                            SendMsg(GameHub.rooms[i].connectionPlayerOne, resultMessage).Wait();

                            GameHub.rooms[i].playerTwo = -1L;
                            GameHub.rooms[i].connectionPlayerTwo = "";
                        }
                    }

                    break;
                case 7:
                    resultMessage = new Message(7, "1");

                    int readyRoomId = message.ReadInt();

                    for (int i = 0; i < GameHub.rooms.Count; i++)
                    {
                        if (GameHub.rooms[i].Id == readyRoomId)
                        {
                            GameHub.rooms[i].isStarted = false;
                            GameHub.rooms[i].isEnd = false;
                            GameHub.rooms[i].isReady = true;
                            await Clients.Clients(Context.ConnectionId, GameHub.rooms[i].connectionPlayerOne).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                            break;
                        }
                    }
                    
                    break;
                case 8:
                    Room roomStart = GetRoomById(int.Parse(message.data));

                    resultMessage = await _gameService.StartRoom(message);                                      

                    await Clients.Clients(Context.ConnectionId, roomStart.connectionPlayerTwo).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
                case 9:
                    resultMessage = await _gameService.Attack(message);
                    int roomAttackId = int.Parse(resultMessage.data.Split('|')[0]);

                    Room roomAttack = GetRoomById(roomAttackId);                 

                    await Clients.Clients(roomAttack.connectionPlayerOne, roomAttack.connectionPlayerTwo).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);

                    break;
                case 10:
                    resultMessage = _gameService.SetMoney(message);
                    int roomSetMoneyId = int.Parse(resultMessage.data.Split('|')[1]);

                    Room roomSetMoney = GetRoomById(roomSetMoneyId);

                    await Clients.Clients(roomSetMoney.connectionPlayerOne, roomSetMoney.connectionPlayerTwo).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
                case 11:
                    resultMessage = _gameService.Chat(message);
                    int roomChatId = int.Parse(resultMessage.data.Split('|')[0]);

                    Room roomChat = GetRoomById(roomChatId);

                    await Clients.Clients(roomChat.connectionPlayerOne, roomChat.connectionPlayerTwo).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
                case 12:
                    resultMessage = await _gameService.ChatGlobal(message);

                    await Clients.All.SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
                case 13:
                    resultMessage = _gameService.CreateRoomBot(message);
                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
                case 14:
                    resultMessage = await _gameService.StartRoomBot(message);

                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    break;
                case 15:
                    resultMessage = await _gameService.AttackBot(message);

                    await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);

                    break;
                case 16:
                    int roomExitBotId = message.ReadInt();
                    try
                    {
                        for (int i = 0; i < GameHub.rooms.Count; i++)
                        {
                            if (GameHub.rooms[i].Id == roomExitBotId)
                            {
                                GameHub.rooms.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    catch
                    {
                    }
                    
                    break;
            }


        }

        public string GetConnectionId(long charId)
        {
            foreach (var item in userLogins)
            {
                if (item.Value == charId)
                {
                    return item.Key;
                }
            }
            return "";
        }

        public Room GetRoomById(int roomId)
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

    }

}
