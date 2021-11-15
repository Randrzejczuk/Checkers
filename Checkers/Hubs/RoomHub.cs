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
                .Include(r => r.User1)
                .Include(r => r.User2)
                .FirstOrDefault();

            string errorMessage = room.ValidatePlayer(userId, move);
            if (errorMessage == "")
                errorMessage = room.Board.ValidateMove(move);
            if (move.Isvalid)
            {
                room.Board.RecordMovement(move);
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
                if ((!room.ActiveUser && room.User1.UserName == "BOT") || (room.ActiveUser && room.User2.UserName == "BOT"))
                    await BotMovement(roomId);
            }
            else
                await Clients.Caller.SendAsync("realizeMovement", move, errorMessage);
        }
        public async Task StartGame(string roomId)
        {
            Room room = _context.Rooms
                .Where(r => r.Id.ToString() == roomId)
                .Include(r=>r.User1)
                .Include(r => r.User2)
                .FirstOrDefault();
            if (room.IsActive)
            {
               // SetTimer(room);
                await Clients.Group(roomId).SendAsync("refresh");
            }
            if (room.User1.UserName == "BOT")
                await BotMovement(roomId);
        }
        public async Task SurrenderGame(string roomId, string userId)
        {
            Room room = _context.Rooms
                .Where(r => r.Id.ToString() == roomId)
                .Include(r=>r.User1)
                .Include(r => r.User2)
                .FirstOrDefault();
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
                        room.IsActive = false;
                        _context.SaveChanges();
                        await Clients.Group(roomId).SendAsync("gameOver", "White player surrendered, Black player wins!", room.User1Id);
                    }
                    else
                    {
                        room.IsActive = false;
                        _context.SaveChanges();
                        await Clients.Group(roomId).SendAsync("gameOver", "Black player surrendered, White player wins!", room.User1Id);
                    }
                }
                else
                {
                    await Clients.Group(roomId).SendAsync("refresh");
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
        private async Task BotMovement(string roomId)
        {
            Room room = _context.Rooms
                .Where(r => r.Id.ToString() == roomId)
                .Include(r=>r.Board.Fields)
                .Include(r => r.User1)
                .Include(r => r.User2)
                .FirstOrDefault();
            string botId = room.User1.UserName == "BOT" ? room.User1Id : room.User2Id;
            Color botColor = room.ActiveUser ? Color.Black : Color.White;
            AiMove move = GetBestMove(true, botColor, new BoardState(room.Board), 4);
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
        private AiMove GetBestMove(bool isAiTurn, Color botColor,BoardState board,int depth)
        {
            List<AiMove> AvailableMoves;
            AiMove selectedMove;
            if (isAiTurn)
                AvailableMoves = board.GetAvailableMoves(botColor);
            else
            {
                Color playerColor = botColor == Color.White ? Color.Black : Color.White;
                AvailableMoves = board.GetAvailableMoves(playerColor);
            }

            //Escape condition
            if (depth == 0)
            {
                foreach (AiMove move in AvailableMoves)
                {
                    BoardState afterMove = new BoardState(board);
                    afterMove = afterMove.RecordMovement(move);
                    Field target = afterMove.GetField(move.TargetX, move.TargetY);
                    if (move.DestroyX != null && target.CanAttack(afterMove))
                        move.Score++;
                }
                selectedMove = AvailableMoves.FirstOrDefault(m => m.Score == AvailableMoves.Max(a => a.Score));
                /*if (isAiTurn)
                    selectedMove = AvailableMoves.FirstOrDefault(m => m.Score == AvailableMoves.Max(a => a.Score));
                else
                    selectedMove = AvailableMoves.FirstOrDefault(m => m.Score == AvailableMoves.Min(a => a.Score));*/
                /*if (!isAiTurn && selectedMove!=null)
                    selectedMove.Score = 0 - selectedMove.Score;*/
                return selectedMove; 
            }
            else
            {
                foreach (AiMove move in AvailableMoves)
                {
                    BoardState afterMove = new BoardState(board);
                    afterMove = afterMove.RecordMovement(move);
                    Field target = afterMove.GetField(move.TargetX, move.TargetY);
                    if (move.DestroyX != null && target.CanAttack(afterMove))
                    {
                        move.Score++;
                        AiMove nextMove = GetBestMove(isAiTurn, botColor, afterMove, depth - 1);
                        if (nextMove!=null)
                        {
                            move.Score += nextMove.Score;
                           /* if (isAiTurn)
                                move.Score += nextMove.Score;
                            else
                                move.Score -= nextMove.Score;*/
                        }
                    }
                    else
                    {
                        AiMove nextMove = GetBestMove(!isAiTurn, botColor, afterMove, depth - 1);
                        if (nextMove != null)
                        {
                            move.Score -= nextMove.Score;
                            /* if (!isAiTurn)
                                 move.Score += nextMove.Score;
                             else
                                 move.Score -= nextMove.Score;*/
                        }
                    }
                }
                selectedMove = AvailableMoves.FirstOrDefault(m => m.Score == AvailableMoves.Max(a => a.Score));
                /*if (isAiTurn)
                        selectedMove = AvailableMoves.FirstOrDefault(m => m.Score == AvailableMoves.Max(a => a.Score));
                    else
                        selectedMove = AvailableMoves.FirstOrDefault(m => m.Score == AvailableMoves.Min(a => a.Score));
                */

                return selectedMove;
            }
        }
    }
}