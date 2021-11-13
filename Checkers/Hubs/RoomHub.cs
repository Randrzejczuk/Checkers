using Checkers.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
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
        public Task JoinRoom(string roomId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }
        public async Task SubmitMove(Move move, string roomId, string userId)
        {
            Room room = _context.Rooms
                .Where(r => r.Id.ToString() == roomId)
                .Include(r => r.Board)
                .Include(r => r.Board.Fields)
                .FirstOrDefault();

            string errorMessage = room.ValidatePlayer(userId, move);
            if (errorMessage == "")
                errorMessage = room.Board.ValidateMove(move);
            if (move.Isvalid)
            {
                if (Math.Abs(move.TargetX - move.StartX) == 2)
                {
                    Field field = room.Board.GetField(move.TargetX, move.TargetY);
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
                        await Clients.Group(roomId).SendAsync("realizeMovement", move, "");
                        break;
                    case Color.Black:
                        room.IsActive = false;
                        _context.SaveChanges();
                        await Clients.Group(roomId).SendAsync("realizeMovement", move, "Black player wins!");
                        break;
                    case Color.White:
                        room.IsActive = false;
                        _context.SaveChanges();
                        await Clients.Group(roomId).SendAsync("realizeMovement", move, "White player wins!");
                        break;
                }
            }
            else
                await Clients.Caller.SendAsync("realizeMovement", move, errorMessage);
        }
        public async Task StartGame(string roomId)
        {
            Room room = _context.Rooms
                .Where(r => r.Id.ToString() == roomId)
                .FirstOrDefault();
            if (room.IsActive)
            {
                SetTimer(room);
                await Clients.Group(roomId).SendAsync("refresh");
            }
        }
        public async Task SurrenderGame(string roomId, string userId)
        {

            Move move = new Move
            {
                Isvalid = false
            };
            Room room = _context.Rooms
                .Where(r => r.Id.ToString() == roomId)
                .FirstOrDefault();
            if (room!=null && room.IsActive)
            {
                if (userId == room.User1Id || room.User1Id == null)
                {
                    room.IsActive = false;
                    _context.SaveChanges();
                    await Clients.Group(roomId).SendAsync("gameOver", "White player surrendered, Black player wins!",room.User1Id);
                }
                else
                {
                    room.IsActive = false;
                    _context.SaveChanges();
                    await Clients.Group(roomId).SendAsync("gameOver", "Black player surrendered, White player wins!",room.User1Id);
                }
            }
            else
            {
                await Clients.Group(roomId).SendAsync("refresh");
            }
        }
    
        private void SetTimer(Room room)
        {
            aTimer.room = room;
            aTimer.CallerContext = Context;
            aTimer.HubCallerClients = Clients;
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
        }
        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            aTimer = (CustomTimer)source;
            HubCallerContext hcallerContext = aTimer.CallerContext;
            IHubCallerClients<IClientProxy> hubClients = aTimer.HubCallerClients;

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            Room room = dbContext.Rooms.Where(r => r.Id == aTimer.room.Id).FirstOrDefault();
            if (room != null && room.IsActive)
            {
                if (!room.ActiveUser)
                    room.User1Time -= TimeSpan.FromSeconds(1);
                else
                    room.User2Time -= TimeSpan.FromSeconds(1);

                if (room.User1Time <= TimeSpan.Zero)
                {
                    aTimer.Enabled = false;
                    room.IsActive = false;
                    dbContext.SaveChanges();
                    await hubClients.Group(room.Id.ToString()).SendAsync("gameOver", "White player ran out of time, Black player wins!");
                }
                else if (room.User2Time <= TimeSpan.Zero)
                {
                    aTimer.Enabled = false;
                    room.IsActive = false;
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
        public async Task SendMessage(string UserId, int? roomId)
        {
            string message = _context.Messages
                  .Where(m => m.UserId == UserId && m.RoomId == roomId)
                  .OrderByDescending(m => m.Posted)
                  .FirstOrDefault()
                  .MessageToDisplay();
            await Clients.Group(roomId.ToString()).SendAsync("receiveMessage", message);
        }
    }

}