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

            for (int i = 0; i < GameHub.rooms.Count; i++)
            {
                if (GameHub.rooms[i].playerOne == userLogins[Context.ConnectionId])
                {
                    if (GameHub.rooms[i].playerTwo != -1)
                    {
                        Message message = new Message(5, "0");
                        SendMsg(GameHub.rooms[i].connectionPlayerTwo, message).Wait();
                    }
                    
                    GameHub.rooms.RemoveAt(i);
                    i--;
                }
                else if (GameHub.rooms[i].playerTwo == userLogins[Context.ConnectionId])
                {
                    Message message = new Message(5, "0");
                    SendMsg(GameHub.rooms[i].connectionPlayerOne, message).Wait();

                    GameHub.rooms[i].playerTwo = -1L;
                    GameHub.rooms[i].connectionPlayerTwo = "";
                }
            }

            userLogins.Remove(Context.ConnectionId);        
            return base.OnDisconnectedAsync(exception);
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

                    if (message.data.StartsWith("1"))
                    {
                        int roomJoinId = int.Parse(message.data.Split('|')[1]);

                        Room roomJoin = GetRoomById(roomJoinId);

                        resultMessage = await _gameService.GetUserById(roomJoin.playerOne);
                        await Clients.Clients(roomJoin.connectionPlayerTwo).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);

                        resultMessage = await _gameService.GetUserById(roomJoin.playerTwo);
                        await Clients.Clients(roomJoin.connectionPlayerOne).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    }               
                    break;
                case 5:
                    resultMessage = _gameService.ReadyRoom(message);
                    if (message.data.StartsWith("0"))
                    {
                        await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    }
                    else
                    {
                        int roomReadyId = int.Parse(message.data.Split('|')[1]);

                        Room roomReady = GetRoomById(roomReadyId);

                        await Clients.Clients(Context.ConnectionId, roomReady.connectionPlayerOne).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    }
                    break;
                case 6:
                    resultMessage = _gameService.StartRoom(message);
                    if (message.data.StartsWith("0"))
                    {
                        await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    }
                    else
                    {
                        int roomStartId = int.Parse(message.data.Split('|')[1]);

                        Room roomStart = GetRoomById(roomStartId);

                        await Clients.Clients(Context.ConnectionId, roomStart.connectionPlayerTwo).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    }
                    break;
                case 7:
                    resultMessage = _gameService.Attack(message);
                    if (message.data.StartsWith("0"))
                    {
                        await Clients.Clients(Context.ConnectionId).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
                    }
                    else
                    {
                        int roomAttackId = int.Parse(message.data.Split('|')[1]);

                        Room roomAttack = GetRoomById(roomAttackId);

                        await Clients.Clients(roomAttack.connectionPlayerOne, roomAttack.connectionPlayerTwo).SendAsync("ReceiveMessage", resultMessage.messageId.ToString(), resultMessage.data);
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
