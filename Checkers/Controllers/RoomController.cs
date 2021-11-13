using Checkers.Data;
using Checkers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Controllers
{
    public class RoomController : Controller
    {
        public readonly ApplicationDbContext _context;
        private readonly ILogger<RoomController> _logger;
        public readonly UserManager<User> _userManager;

        public RoomController(ILogger<RoomController> logger, ApplicationDbContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        public Room GetRoom(int roomId)
        {
            Room room = _context.Rooms
                .Where(r => r.Id == roomId)
                .Include(r => r.Board)
                .Include(r => r.Board.Fields)
                .Include(r => r.User1)
                .Include(r => r.User2)
                .Include(r => r.Messages)
                .FirstOrDefault();
            if (room != null)
                return room;
            else
                throw new Exception($"Room whith Id {roomId} does not exist.");
        }
        [Authorize]
        public IActionResult List()
        {
            var result = _context.Rooms
                .Include(u => u.User1)
                .Include(u => u.User2);
            return View(result);
        }
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        [Authorize]

        public IActionResult Create()
        {
            return View();
        }
        [ActionName("Create room")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateAsync(Room room)
        {
            if (ModelState.IsValid)
            {
                room.Board = new BoardState();
                room.Board.Init();
                var user = await _userManager.GetUserAsync(User);
                room.User1 = user;
                room.User1Id = user.Id;
                room.User1IsActive = false;
                room.User1Time = TimeSpan.FromMinutes(5);
                room.User2IsActive = false;
                room.User2Time = TimeSpan.FromMinutes(5);
                room.ActiveUser = false;
                room.IsActive = false;
                _context.Add(room);
                _context.SaveChanges();
            }
            return RedirectToAction("Room", new { roomId = room.Id });
        }
        [Authorize]
        public IActionResult Room(int roomId)
        {
            try
            {
                Room room = GetRoom(roomId);
                room.Messages = room.Messages.OrderBy(m => m.Posted).TakeLast(15).ToList();
                var userId = _userManager.GetUserId(User);
                ViewBag.currentUserId = userId;
                return View(room);
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                return RedirectToAction("List");
            }
        }
        public async Task<IActionResult> StartButton(int roomId)
        {
            try
            {
                Room room = GetRoom(roomId);
                var user = await _userManager.GetUserAsync(User);
                if (user.Id == room.User1Id)
                {
                    room.User1IsActive = true;
                    _context.SaveChanges();
                }

                else if (user.Id == room.User2Id)
                {
                    room.User2IsActive = true;
                    _context.SaveChanges();
                }
                if (room.User1IsActive && room.User2IsActive)
                {
                    room.User1IsActive = false;
                    room.User1Time = TimeSpan.FromMinutes(5);
                    room.User2IsActive = false;
                    room.User2Time = TimeSpan.FromMinutes(5);
                    room.ActiveUser = false;
                    room.IsActive = true;
                    room.Board.Init();
                    _context.SaveChanges();
                }
                return RedirectToAction("Room", new { roomId });
            }
            catch
            {
                return RedirectToAction("List");
            }
        }
        public async Task<IActionResult> JoinButton(int roomId, int buttonId)
        {
            try
            {
                Room room = GetRoom(roomId);
                var user = await _userManager.GetUserAsync(User);
                if (buttonId == 1)
                {
                    if (room.User1 == null)
                    {
                        room.User1 = user;
                        room.User1Id = user.Id;
                        if (room.User2Id == user.Id)
                        {
                            room.User2 = null;
                            room.User2Id = null;
                            room.User2IsActive = false;
                        }
                    }
                    else if (room.User1.Id == user.Id)
                    {
                        room.User1 = null;
                        room.User1Id = null;
                        room.User1IsActive = false;
                        if (room.User2 == null)
                        {
                            _context.Messages.RemoveRange(room.Messages);
                            _context.Rooms.Remove(room);
                            _context.SaveChanges();
                            return RedirectToAction("List");
                        }
                    }
                }
                else
                {
                    if (room.User2 == null)
                    {
                        room.User2 = user;
                        room.User2Id = user.Id;
                        if (room.User1Id == user.Id)
                        {
                            room.User1 = null;
                            room.User1Id = null;
                            room.User1IsActive = false;
                        }
                    }
                    else if (room.User2.Id == user.Id)
                    {
                        room.User2 = null;
                        room.User2Id = null;
                        room.User2IsActive = false;
                        if (room.User1 == null)
                        {
                            _context.Rooms.Remove(room);
                            _context.SaveChanges();
                            return RedirectToAction("List");
                        }
                    }
                }
                _context.SaveChanges();
                return RedirectToAction("Room", new { roomId });
            }
            catch
            {
                return RedirectToAction("List");
            }
        }
        [Authorize]
        public async Task<IActionResult> AddMessage(string userId, string text, int roomId)
        {
            Message message = new Message()
            {
                RoomId = roomId,
                Text = text,
                UserId = userId,
                UserName = User.Identity.Name,
                Posted = DateTime.Now,
            };
                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();
                return Ok();
        }
    }
}
