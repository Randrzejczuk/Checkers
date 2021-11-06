using Checkers.Data;
using Checkers.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Hubs
{
    public class ChatHub : Hub
    {
        public readonly ApplicationDbContext _context;
        public CustomTimer aTimer = new CustomTimer(1000);
        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SendMessage(string UserId)
        {
          string message = _context.Messages
                .Where(m => m.UserId == UserId)
                .OrderByDescending(m=>m.Posted)
                .FirstOrDefault()
                .MessageToDisplay();
            await Clients.All.SendAsync("receiveMessage", message);
        }
    }


}
