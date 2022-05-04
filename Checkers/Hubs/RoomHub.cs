using Checkers.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using Checkers.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Checkers.Hubs
{
    public class RoomHub : Hub
    {
        public readonly ApplicationDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public CustomTimer aTimer = new CustomTimer(1000);
        public RoomHub(ApplicationDbContext context,IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }
        /*
         * Sends info to every user in room, that player joined
         */
        public Task JoinRoom(string roomId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }
        /*
         * Returns room from database by its roomId
         */
        private Room GetRoom(int roomId)
        {
            Room room = _context.Rooms
                .Include(r => r.Board)
                .Include(r => r.Board.Fields)
                .Include(r => r.User1)
                .Include(r => r.User2)
                .FirstOrDefault(r => r.Id == roomId);
            return room;
        }
        /*
         * Validates processes move request
         */
        public async Task SubmitMove(Move move, int roomId, string userId)
        {
            Room room = GetRoom(roomId);

            string errorMessage = room.ValidatePlayer(userId, move);
            if (errorMessage == "")
                errorMessage = room.Board.ValidateMove(move);
            if (move.Isvalid)
            {
                room.Board.RecordMovement(move);
                if (Math.Abs(move.TargetX - move.StartX) == 2)
                {
                    Field field = room.Board[move.TargetX, move.TargetY];
                    if (!field.CanAttack(room.Board))
                        room.ActiveUser = !room.ActiveUser;
                }
                else
                    room.ActiveUser = !room.ActiveUser;
                _context.SaveChanges();
                Color winner = room.Board.CheckWinner();
                switch (winner)
                {
                    case Color.None:
                        await Clients.Group(roomId.ToString()).SendAsync("realizeMovement", move, "");
                        break;
                    case Color.Black:
                        room.gameOver();
                        _context.SaveChanges();
                        await Clients.Group(roomId.ToString()).SendAsync("realizeMovement", move, "Black player wins!");
                        break;
                    case Color.White:
                        room.gameOver();
                        _context.SaveChanges();
                        await Clients.Group(roomId.ToString()).SendAsync("realizeMovement", move, "White player wins!");
                        break;
                }
                if ((!room.ActiveUser && room.User1.UserName == "BOT") || (room.ActiveUser && room.User2.UserName == "BOT"))
                    await BotMovement(roomId);
            }
            else
                await Clients.Caller.SendAsync("realizeMovement", move, errorMessage);
        }
        /*
         * Sends info to players that game has started
         */
        public async Task StartGame(int roomId)
        {
            Room room = GetRoom(roomId);
            if (room.IsActive)
            {
                SetTimer(room);
                await Clients.Group(roomId.ToString()).SendAsync("refresh");
            }
            if (room.User1.UserName == "BOT")
                await BotMovement(roomId);
        }
        /*
         * Sends info to players that one of them surrendered
         */
        public async Task SurrenderGame(int roomId, string userId)
        {
            Room room = GetRoom(roomId);
            if (room != null)
            {
                if (room.User1 != null && room.User1.UserName == "BOT")
                {
                    room.User1 = null;
                }
                else if (room.User2 != null && room.User2.UserName == "BOT")
                {
                    room.User2 = null;
                }
                _context.SaveChanges();

                if (room.IsActive)
                {
                    if (userId == room.User1Id || room.User1Id == null)
                    {
                        room.gameOver();
                        _context.SaveChanges();
                        await Clients.Group(roomId.ToString()).SendAsync("gameOver", "White player surrendered, Black player wins!", room.User1Id);
                    }
                    else
                    {
                        room.gameOver();
                        _context.SaveChanges();
                        await Clients.Group(roomId.ToString()).SendAsync("gameOver", "Black player surrendered, White player wins!", room.User1Id);
                    }
                }
                else
                {
                    await Clients.Group(roomId.ToString()).SendAsync("refresh");
                }
            }
            else
            {
                await Clients.Group(roomId.ToString()).SendAsync("refresh");
            }
        }
        /*
         * Sets up timer for room
         */
        private void SetTimer(Room room)
        {
            aTimer.room = room;
            aTimer.CallerContext = Context;
            aTimer.HubCallerClients = Clients;
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
        }
        /*
         * Removes one second from timer
         */
        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            aTimer = (CustomTimer)source;
            HubCallerContext hcallerContext = aTimer.CallerContext;
            IHubCallerClients<IClientProxy> hubClients = aTimer.HubCallerClients;

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            Room room = dbContext.Rooms
                .Where(r => r.Id == aTimer.room.Id)
                .Include(r => r.Board)
                .Include(r => r.Board.Fields)
                .Include(r => r.User1)
                .Include(r => r.User2)
                .FirstOrDefault();
            if (room != null && room.IsActive)
            {
                if (!room.ActiveUser)
                    room.User1Time -= TimeSpan.FromSeconds(1);
                else
                    room.User2Time -= TimeSpan.FromSeconds(1);

                if (room.User1Time <= TimeSpan.Zero)
                {
                    //aTimer.Enabled = false;
                    room.gameOver();
                    dbContext.SaveChanges();
                    await hubClients.Group(room.Id.ToString()).SendAsync("gameOver", "White player ran out of time, Black player wins!");
                }
                else if (room.User2Time <= TimeSpan.Zero)
                {
                    //aTimer.Enabled = false;
                    room.gameOver();
                    dbContext.SaveChanges();
                    await hubClients.Group(room.Id.ToString()).SendAsync("gameOver", "Black player ran out of time, White player wins!");
                }
                else
                {
                    dbContext.SaveChanges();
                    await hubClients.Group(room.Id.ToString()).SendAsync("updateTime", room.User1Time.ToString(), room.User2Time.ToString());
                }
            }
            else
                aTimer.Enabled = false;
        }
        /*
         * Sends message to users of the room
         */
        public async Task SendMessage(string UserId, int? roomId)
        {
            string message = _context.Messages
                  .Where(m => m.UserId == UserId && m.RoomId == roomId)
                  .OrderByDescending(m => m.Posted)
                  .FirstOrDefault()
                  .MessageToDisplay();
            await Clients.Group(roomId.ToString()).SendAsync("receiveMessage", message);
        }
        /*
         * Prosesses move done by AI
         */
        private async Task BotMovement(int roomId)
        {
            Room room = GetRoom(roomId);
            string botId = room.User1.UserName == "BOT" ? room.User1Id : room.User2Id;
            Color botColor = room.ActiveUser ? Color.Black : Color.White;
            AiMove move = room.Board.GetBestMove(true, botColor, new BoardState(room.Board), 4);
            if (move!=null)
            {
                Thread.Sleep(1000);
                await SubmitMove(move, roomId,botId);
            }
            else
            {
               await SurrenderGame(roomId, botId);
            }
        }
    }
}